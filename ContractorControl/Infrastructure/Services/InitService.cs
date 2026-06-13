using ContractorControl.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ContractorControl.Infrastructure.Services;

public class InitService
{
    private readonly IApplicationDbContext _context;
    public InitService(IApplicationDbContext context) => _context = context;

    public async Task InitializeDatabaseAsync(Dictionary<string, string> files)
    {
        var db = ((DbContext)_context).Database;
        await db.ExecuteSqlRawAsync("CREATE SCHEMA IF NOT EXISTS contractor_control;");
        var isExistDb = await db.EnsureCreatedAsync();
        if (isExistDb)
        {
            await CreateStoredProceduresAsync(files);
        }
    }

    private async Task CreateStoredProceduresAsync(Dictionary<string, string> files)
    {
        /*
         * Соответствие алгоритму:
         * Step 1-3: Реализованы точно по описанию через SELECT ... INTO. Проверка delete_at IS NULL добавлена везде, где требуется работа с активными (не удаленными) записями.
         * Step 4: Реализована как проверки IF ... IS NULL THEN RETURN FALSE после каждого шага.
         * Step 5: Используется array_agg для сбора всех state_source_id в массив.
         * Step 6: Реализован через цикл FOREACH. Внутри цикла выполняется логика проверки: "Выполнен ли предшественник?". Так как "выполнен" в контексте DAG обычно означает наличие события со статусом SUCCESS, реализована проверка наличия такого события в таблице events. Если вход в функцию был проверкой "можно ли установить", то условие возможности установки — завершенность всех родителей.
         * Step 7: Если цикл завершился без возврата FALSE, значит все условия выполнены, возвращаем TRUE.
         * Обработка орфографии:
         * Исправлены опечатки из задания (conract_id -> contract_id, daleteat -> delete_at) на корректные имена колонок PostgreSQL.
         * Оптимизация:
         * Использован LIMIT 1 в подзапросах для гарантии единственности записи.
         * Использованы индексы (предполагается их наличие согласно структуре БД), что делает функцию быстрой.
        */
        var spCheckStateToSet = await ReadEmbeddedScriptAsync(files["spCheckStateToSet"]);

        /*
         * 
         * Пояснения к реализации:
         * Step 1: Выполняет SELECT id из таблицы contract_execution по переданному p_instance_guid. Если запись не найдена (или помечена как удаленная delete_at IS NULL), функция возвращает FALSE.
         * Step 2: Выполняет проверку существования записи в events. Если найдена хотя бы одна запись, связывающая данный запуск контракта с состоянием "FINISHED", функция возвращает TRUE.
         * 
        */
        var spCheckIsFinished = await ReadEmbeddedScriptAsync(files["spCheckIsFinished"]);

        await ((DbContext)_context).Database.ExecuteSqlRawAsync(spCheckStateToSet);
        await ((DbContext)_context).Database.ExecuteSqlRawAsync(spCheckIsFinished);
    }

    /// <summary>
    /// Читает содержимое встроенного ресурса по имени.
    /// </summary>
    private async Task<string> ReadEmbeddedScriptAsync(string resourceName)
    {
        string basePath = Directory.GetCurrentDirectory();
        // Путь к файлу
        string pathCheckFinished = Path.Combine(basePath, "Application", "Scripts", resourceName);

        // Читаем файл
        if (File.Exists(pathCheckFinished))
        {
            string sql = await File.ReadAllTextAsync(pathCheckFinished);
            return sql;
        }

        return string.Empty;
    }
}
