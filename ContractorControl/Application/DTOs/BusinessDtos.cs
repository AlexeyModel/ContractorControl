namespace ContractorControl.Application.DTOs;

public class ContractExecutionCreateDto
{
    public string Code { get; set; } = string.Empty;
    public int ExecutionTimeout { get; set; }
}

public class SetStateDto
{
    public Guid InstanceContractExecution { get; set; }
    public string StateName { get; set; } = string.Empty;
    public string StateType { get; set; } = string.Empty;
}

public class CheckStateDto
{
    public Guid InstanceContractExecution { get; set; }
    public string StateName { get; set; } = string.Empty;
    public string StateType { get; set; } = string.Empty;
}

public class InstanceDto
{
    public Guid InstanceContractExecution { get; set; }
}
