namespace ContractorControl.Application.DTOs;

using System.Text.Json.Serialization;

public class ApiResponse
{
    public bool Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public object Data { get; set; } = new();
}

public class InstanceResponseData
{
    public Guid InstanceContractExecution { get; set; }
}

public class EventResponseDto
{
    [JsonPropertyName("instance_contract_execution")]
    public Guid InstanceContractExecution { get; set; }

    [JsonPropertyName("create_at")]
    public DateTime CreateAt { get; set; }

    [JsonPropertyName("update_at")]
    public DateTime? UpdateAt { get; set; }

    [JsonPropertyName("delete_at")]
    public DateTime? DeleteAt { get; set; }

    [JsonPropertyName("contract_id")]
    public int ContractId { get; set; }

    [JsonPropertyName("state_name")]
    public string StateName { get; set; } = string.Empty;

    [JsonPropertyName("state_type")]
    public string StateType { get; set; } = string.Empty;
}
