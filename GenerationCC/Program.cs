using System.Security.Cryptography;
using System.Text.Json.Nodes;

// ==========================================
// generation_cc - Key Generator Utility
// ==========================================

if (args.Length == 0)
{
    PrintHelp();
    return;
}

try
{
    switch (args)
    {
        case ["-new"]:
            GenerateAndPrintKey();
            break;

        case ["-f", var path, "-update"]:
            await UpdateKeyInFileAsync(path);
            break;

        case ["-h"] or ["--help"] or ["help"]:
            PrintHelp();
            break;

        default:
            PrintError($"Unknown command: {string.Join(' ', args)}");
            PrintHelp();
            break;
    }
}
catch (Exception ex)
{
    PrintError(ex.Message);
}

// ==========================================
// Logic Implementation
// ==========================================

static void GenerateAndPrintKey()
{
    string newKey = GenerateSecretKey();
    Console.WriteLine(newKey);
}

static async Task UpdateKeyInFileAsync(string filePath)
{
    if (!File.Exists(filePath))
    {
        throw new FileNotFoundException($"File not found: {filePath}");
    }

    // Чтение файла с сохранением переносов строк (для кроссплатформенности)
    string jsonContent = await File.ReadAllTextAsync(filePath);

    // Парсинг JSON
    var node = JsonNode.Parse(jsonContent,
        nodeOptions: new JsonNodeOptions { PropertyNameCaseInsensitive = true });

    if (node is null)
    {
        throw new InvalidOperationException("Failed to parse JSON file.");
    }

    // Генерация нового ключа
    string newKey = GenerateSecretKey();

    // Поиск и обновление ключа
    // Поддержка как корневого объекта, так и вложенных секций (например, ConnectionStrings)
    bool updated = TryUpdateNode(node, newKey);

    if (!updated)
    {
        throw new KeyNotFoundException("Key 'SecretKey' not found in the JSON structure.");
    }

    // Сохранение изменений с сохранением форматирования
    var options = new System.Text.Json.JsonSerializerOptions
    {
        WriteIndented = true
    };

    string newJsonContent = node.ToJsonString(options);

    // Замена стандартных Escape-последовательностей для чистоты файла (опционально)
    // newJsonContent = newJsonContent.Replace("\\u0022", "\""); 

    await File.WriteAllTextAsync(filePath, newJsonContent);

    Console.WriteLine($"Success! 'SecretKey' updated in: {Path.GetFullPath(filePath)}");
    Console.WriteLine($"New Key: {newKey}");
}

static bool TryUpdateNode(JsonNode node, string newValue)
{
    // Если это объект, ищем свойство SecretKey
    if (node is JsonObject obj)
    {
        if (obj.ContainsKey("SecretKey"))
        {
            obj["SecretKey"] = newValue;
            return true;
        }

        // Рекурсивный поиск в дочерних объектах
        foreach (var property in obj)
        {
            if (property.Value is not null && TryUpdateNode(property.Value, newValue))
            {
                return true;
            }
        }
    }

    return false;
}

static string GenerateSecretKey()
{
    // Генерация криптографически стойкого ключа (256 бит = 32 байта)
    // Кодируем в Base64 для использования в конфигурации
    byte[] bytes = RandomNumberGenerator.GetBytes(32);
    return Convert.ToBase64String(bytes);
}

static void PrintHelp()
{
    string exeName = "generation_cc";

    Console.WriteLine($@"
Usage:
  {exeName} -new
      Generate a new secret key and print to console.

  {exeName} -f <path_url> -update
      Update the 'SecretKey' value in the specified JSON file.

Options:
  -new         Generate key mode.
  -f           File path mode.
  -update      Action to update the key in the file.
");
}

static void PrintError(string message)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"Error: {message}");
    Console.ResetColor();
}