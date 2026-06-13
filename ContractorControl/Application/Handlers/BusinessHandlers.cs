using ContractorControl.Application.Commands;
using ContractorControl.Application.DTOs;
using ContractorControl.Application.Interfaces;
using ContractorControl.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ContractorControl.Application.Handlers;

public class CreateContractExecutionHandler : IRequestHandler<CreateContractExecutionCommand, ApiResponse>
{
    private readonly IApplicationDbContext _db;
    public CreateContractExecutionHandler(IApplicationDbContext db) => _db = db;

    public async Task<ApiResponse> Handle(CreateContractExecutionCommand request, CancellationToken ct)
    {
        var contract = await _db.Contracts.FirstOrDefaultAsync(c => c.Code == request.Dto.Code, ct);
        if (contract == null)
            return new ApiResponse { Status = false, Message = $"ERROR: Object name '{request.Dto.Code}' is not found" };

        var execution = new ContractExecution
        {
            ContractId = contract.Id,
            InstanceContractExecution = Guid.NewGuid()
        };

        _db.ContractExecutions.Add(execution);
        await _db.SaveChangesAsync(ct);

        return new ApiResponse { Status = true, Data = new InstanceResponseData { InstanceContractExecution = execution.InstanceContractExecution } };
    }
}

public class SetStateHandler : IRequestHandler<SetStateCommand, ApiResponse>
{
    private readonly IApplicationDbContext _db;
    public SetStateHandler(IApplicationDbContext db) => _db = db;

    public async Task<ApiResponse> Handle(SetStateCommand request, CancellationToken ct)
    {
        var dto = request.Dto;
        var exec = await _db.ContractExecutions.FirstOrDefaultAsync(e => e.InstanceContractExecution == dto.InstanceContractExecution, ct);
        if (exec == null) return new ApiResponse { Status = false, Message = "Execution instance not found" };

        var stateType = await _db.StateTypes.FirstOrDefaultAsync(st => st.Type == dto.StateType, ct);
        if (stateType == null) return new ApiResponse { Status = false, Message = "StateType not found" };

        var state = await _db.States.FirstOrDefaultAsync(s => s.Name == dto.StateName && s.ContractId == exec.ContractId && s.StateTypeId == stateType.Id, ct);
        if (state == null) return new ApiResponse { Status = false, Message = "State not found in contract" };

        var newEvent = new Event { ContractExecutionId = exec.Id, StateId = state.Id };
        _db.Events.Add(newEvent);
        await _db.SaveChangesAsync(ct);

        return new ApiResponse { Status = true };
    }
}
