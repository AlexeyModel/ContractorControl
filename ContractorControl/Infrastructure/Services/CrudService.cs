using ContractorControl.Application.DTOs;
using ContractorControl.Application.Services;
using ContractorControl.Domain.Common;
using ContractorControl.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ContractorControl.Infrastructure.Services;

public class CrudService : ICrudService
{
    private readonly ApplicationDbContext _context;
    public CrudService(ApplicationDbContext context) => _context = context;

    public async Task<ApiResponse> InsertAsync(SetPropertyInfo body)
    {
        var dbSetProperty = _context.GetType().GetProperties()
            .FirstOrDefault(p => p.Name.Equals(body.TableName, StringComparison.OrdinalIgnoreCase));
        if (dbSetProperty == null) return new ApiResponse { Status = false, Message = "Table not found" };

        var entityType = dbSetProperty.PropertyType.GenericTypeArguments[0];
        var instance = Activator.CreateInstance(entityType);
        if (instance == null) return new ApiResponse { Status = false, Message = "Entity creation failed" };

        foreach (var prop in body.Data.EnumerateObject())
        {
            var entityProp =
                entityType
                    .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                    .FirstOrDefault(p =>
                        p.GetCustomAttributes(typeof(ColumnAttribute), true)
                            .Cast<ColumnAttribute>()
                            .Any(attr => string.Equals(attr.Name, prop.Name, StringComparison.OrdinalIgnoreCase))
                    );

            // Если атрибут Column не задан, ищем по обычному имени свойства (запасной вариант)
            if (entityProp == null)
            {
                entityProp = entityType.GetProperty(prop.Name, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            }

            if (entityProp is { CanWrite: true } && prop.Value.ValueKind is not (JsonValueKind.Object or JsonValueKind.Array))
            {
                var options = new JsonSerializerOptions
                {
                    NumberHandling = JsonNumberHandling.AllowReadingFromString
                };

                // Прямая десериализация в целевой тип свойства
                var value = JsonSerializer.Deserialize(prop.Value.GetRawText(), entityProp.PropertyType, options);
                entityProp.SetValue(instance, value);
            }
        }

        dbSetProperty.GetValue(_context)?.GetType().GetMethod("Add")?.Invoke(dbSetProperty.GetValue(_context), new[] { instance });
        await _context.SaveChangesAsync(default);
        return new ApiResponse { Status = true };
    }

    public async Task<ApiResponse> UpdateAsync(SetPropertyInfo body)
    {
        var entity = await FindEntityAsync(body.TableName, body.Id);
        if (entity == null) return new ApiResponse { Status = false, Message = "Object not found" };

        var entityType = entity.GetType();

        var delProp = entity.GetType().GetProperty("DeleteAt");
        if (delProp?.GetValue(entity) != null) return new ApiResponse { Status = false, Message = "Cannot update deleted object" };

        foreach (var prop in body.Data.EnumerateObject())
        {
            var entityProp =
                entityType
                    .GetProperties(System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                    .FirstOrDefault(p =>
                        p.GetCustomAttributes(typeof(ColumnAttribute), true)
                            .Cast<ColumnAttribute>()
                            .Any(attr => string.Equals(attr.Name, prop.Name, StringComparison.OrdinalIgnoreCase))
                    );

            // Если атрибут Column не задан, ищем по обычному имени свойства (запасной вариант)
            if (entityProp == null)
            {
                entityProp = entityType.GetProperty(prop.Name, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            }

            if (entityProp is { CanWrite: true } && prop.Value.ValueKind is not (JsonValueKind.Object or JsonValueKind.Array))
            {
                var options = new JsonSerializerOptions
                {
                    NumberHandling = JsonNumberHandling.AllowReadingFromString
                };

                // Прямая десериализация в целевой тип свойства
                var value = JsonSerializer.Deserialize(prop.Value.GetRawText(), entityProp.PropertyType, options);
                entityProp.SetValue(entity, value);
            }
        }

        await _context.SaveChangesAsync(default);
        return new ApiResponse { Status = true };
    }

    public async Task<ApiResponse> SoftDeleteAsync(SetPropertyInfo body)
    {
        var entity = await FindEntityAsync(body.TableName, body.Id);
        if (entity == null) return new ApiResponse { Status = false, Message = "Object not found" };

        var delProp = entity.GetType().GetProperty("DeleteAt");
        if (delProp?.GetValue(entity) != null) return new ApiResponse { Status = false, Message = "Already deleted" };

        delProp?.SetValue(entity, DateTime.UtcNow);
        _context.Entry(entity).State = EntityState.Modified; 
        await _context.SaveChangesAsync(default);
        return new ApiResponse { Status = true };
    }

    private async Task<object?> FindEntityAsync(string tableName, int id)
    {
        // Получаем тип сущности напрямую из модели EF по имени таблицы/свойства
        var dbSetProperty = _context.GetType().GetProperties()
            .FirstOrDefault(p => p.Name.Equals(tableName, StringComparison.OrdinalIgnoreCase));
        if (dbSetProperty == null) return new ApiResponse { Status = false, Message = "Table not found" };

        var entityType = dbSetProperty.PropertyType.GenericTypeArguments[0];

        if (entityType == null) return null;

        // Используем встроенный асинхронный поиск DbContext по типу и Id
        return await _context.FindAsync(entityType, id);
    }
}
