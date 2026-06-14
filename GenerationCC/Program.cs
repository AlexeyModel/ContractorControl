using Npgsql;

namespace GenerationCC
{
    class Program
    {
        class StateInfo
        {
            public string Name { get; set; } = string.Empty;
            public string Type { get; set; } = string.Empty;

            public override string ToString() => $"'{Name}' с типом '{Type}'";
        }

        class Dependency
        {
            public StateInfo Parent { get; set; } = new();
            public StateInfo Child { get; set; } = new();
        }

        class StackItem
        {
            public int IndentLevel { get; set; }
            public StateInfo State { get; set; } = new();
        }

        static async Task<int> Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Использование: DagLoader <ConnectionString> <ContractId> <FilePath>");
                Console.WriteLine("Пример: DagLoader \"Host=localhost;Username=postgres;Password=secret;Database=my_db\" 42 \"my_dag.txt\"");
                return 1;
            }

            string connectionString = args[0];
            if (!int.TryParse(args[1], out int contractId))
            {
                Console.WriteLine("[!] Ошибка: ContractId должен быть числом.");
                return 1;
            }
            string filePath = args[2];

            if (!File.Exists(filePath))
            {
                Console.WriteLine($"[!] Ошибка: Файл '{filePath}' не найден.");
                return 1;
            }

            Console.WriteLine($"[*] Чтение и анализ файла: {filePath}");
            List<Dependency> dependencies = ParseDagFile(filePath);

            if (dependencies.Count == 0)
            {
                Console.WriteLine("[!] В файле не обнаружено связей или файл пуст.");
                return 0;
            }

            await LoadDagToDatabaseAsync(connectionString, contractId, dependencies);
            return 0;
        }

        private static List<Dependency> ParseDagFile(string filePath)
        {
            var dependencies = new List<Dependency>();
            var stack = new Stack<StackItem>();

            foreach (var line in File.ReadLines(filePath))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                string trimmedLine = line.TrimEnd();
                int indentLevel = trimmedLine.Length - trimmedLine.TrimStart().Length;
                string rawContent = trimmedLine.Trim();

                string[] parts = rawContent.Split(new[] { '-' }, 2);
                if (parts.Length < 2)
                {
                    Console.WriteLine($"[!] Пропущена некорректная строка (нет дефиса): '{rawContent}'");
                    continue;
                }

                var stateInfo = new StateInfo
                {
                    Name = parts[0].Trim(),
                    Type = parts[1].Trim()
                };

                while (stack.Count > 0 && stack.Peek().IndentLevel >= indentLevel)
                {
                    stack.Pop();
                }

                if (stack.Count > 0)
                {
                    dependencies.Add(new Dependency
                    {
                        Parent = stack.Peek().State,
                        Child = stateInfo
                    });
                }

                stack.Push(new StackItem { IndentLevel = indentLevel, State = stateInfo });
            }

            return dependencies;
        }

        private static async Task LoadDagToDatabaseAsync(string connectionString, int contractId, List<Dependency> dependencies)
        {
            int insertedCount = 0;

            // HashSet для сбора уникальных названий состояний, которые отсутствуют в БД
            var missingStatesReport = new HashSet<string>();

            try
            {
                await using var conn = new NpgsqlConnection(connectionString);
                await conn.OpenAsync();
                Console.WriteLine($"[*] Подключение успешно. Обработка связей для contract_id: {contractId}\n");

                string selectStateSql = @"
                    SELECT s.id 
                    FROM contractor_control.states s
                    JOIN contractor_control.state_type t ON s.state_type_id = t.id
                    WHERE s.contract_id = @contract_id 
                      AND s.name = @name 
                      AND t.type = @type
                      AND s.delete_at IS NULL 
                      AND t.delete_at IS NULL
                    LIMIT 1;";

                string insertDagSql = @"
                    INSERT INTO contractor_control.dags (state_source_id, state_destination_id, create_at)
                    VALUES (@source_id, @dest_id, NOW())
                    ON CONFLICT (state_source_id, state_destination_id) WHERE (delete_at IS NULL) 
                    DO NOTHING;";

                await using var transaction = await conn.BeginTransactionAsync();

                foreach (var dep in dependencies)
                {
                    int? parentId = null;
                    int? childId = null;

                    // 1. Поиск родителя
                    await using (var cmd = new NpgsqlCommand(selectStateSql, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("contract_id", contractId);
                        cmd.Parameters.AddWithValue("name", dep.Parent.Name);
                        cmd.Parameters.AddWithValue("type", dep.Parent.Type);
                        var result = await cmd.ExecuteScalarAsync();
                        if (result != null && result != DBNull.Value) parentId = Convert.ToInt32(result);
                    }

                    // 2. Поиск потомка
                    await using (var cmd = new NpgsqlCommand(selectStateSql, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("contract_id", contractId);
                        cmd.Parameters.AddWithValue("name", dep.Child.Name);
                        cmd.Parameters.AddWithValue("type", dep.Child.Type);
                        var result = await cmd.ExecuteScalarAsync();
                        if (result != null && result != DBNull.Value) childId = Convert.ToInt32(result);
                    }

                    // Логируем ошибки в отчет, если что-то не нашли
                    if (!parentId.HasValue)
                    {
                        missingStatesReport.Add(dep.Parent.ToString());
                    }
                    if (!childId.HasValue)
                    {
                        missingStatesReport.Add(dep.Child.ToString());
                    }

                    // Если одного из узлов нет, связь физически невозможно создать
                    if (!parentId.HasValue || !childId.HasValue)
                    {
                        continue;
                    }

                    // 3. Запись связи в dags
                    await using (var cmd = new NpgsqlCommand(insertDagSql, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("source_id", parentId.Value);
                        cmd.Parameters.AddWithValue("dest_id", childId.Value);

                        int rowsAffected = await cmd.ExecuteNonQueryAsync();
                        if (rowsAffected > 0)
                        {
                            insertedCount++;
                            Console.WriteLine($"[+] Создана связь: {dep.Parent.Name} -> {dep.Child.Name}");
                        }
                    }
                }

                await transaction.CommitAsync();

                // Финальные итоги в консоли
                Console.WriteLine($"\n[└─] Успешно добавлено новых связей: {insertedCount}");

                // Вывод детального лога ошибок, если они были обнаружены
                if (missingStatesReport.Count > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"\n[!] ВНИМАНИЕ: Обнаружено {missingStatesReport.Count} состояний, которых нет в таблице 'states':");
                    foreach (var missingState in missingStatesReport.OrderBy(x => x))
                    {
                        Console.WriteLine($"   - {missingState}");
                    }
                    Console.ResetColor();
                    Console.WriteLine("\n[💡] Связи для этих состояний не были записаны в dags. Сначала добавьте их в базу.");
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n[!] Критическая ошибка БД: {ex.Message}");
                Console.ResetColor();
            }
        }
    }
}
