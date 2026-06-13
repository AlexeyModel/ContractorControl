using System.Text.Json;

namespace ContractorControl.Application.DTOs;

public class CrudRequestDto
{
    public string TableName { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public JsonElement Data { get; set; }
    public int? Id { get; set; }
}
