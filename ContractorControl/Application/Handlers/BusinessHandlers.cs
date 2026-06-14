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

        // 1. Поиск исполнения контракта
        var exec = await _db.ContractExecutions
            .FirstOrDefaultAsync(e => e.InstanceContractExecution == dto.InstanceContractExecution && e.DeleteAt == null, ct);
        if (exec == null) return new ApiResponse { Status = false, Message = "Execution instance not found" };

        // 2. Поиск типа состояния
        var stateType = await _db.StateTypes
            .FirstOrDefaultAsync(st => st.Type == dto.StateType && st.DeleteAt == null, ct);
        if (stateType == null) return new ApiResponse { Status = false, Message = "StateType not found" };

        // 3. Поиск самого состояния в рамках контракта
        var state = await _db.States
            .FirstOrDefaultAsync(s => s.Name == dto.StateName && s.ContractId == exec.ContractId && s.StateTypeId == stateType.Id && s.DeleteAt == null, ct);
        if (state == null) return new ApiResponse { Status = false, Message = "State not found in contract" };

        // 4. Проверка DAG (выполняется ТОЛЬКО если статус НЕ является GLOBAL)
        // Глобальные статусы имеют StateTypeId == 1 и ставятся ВСЕГДА
        if (state.StateTypeId != 1)
        {
            // Ищем, есть ли у целевого состояния такие предшественники (родители) в dags,
            // для которых в таблице events ЕЩЕ НЕТ записи для текущего исполнения контракта
            bool hasUnresolvedPredecessors = await _db.Dags
                .Where(d => d.StateDestinationId == state.Id && d.DeleteAt == null)
                .AnyAsync(d => !_db.Events.Any(e =>
                    e.StateId == d.StateSourceId &&
                    e.ContractExecutionId == exec.Id &&
                    e.DeleteAt == null), ct);

            // Если найден хотя бы один незавершенный родительский узел -> блокируем установку.
            // Если связей в dags для данного state.Id нет вообще, AnyAsync вернет false,
            // и проверка успешно пройдет (статус поставится в любом порядке).
            if (hasUnresolvedPredecessors)
            {
                return new ApiResponse { Status = false, Message = " cannot be set: DAG predecessors are not completed" };
            }
        }

        // 5. Создание и сохранение события
        var newEvent = new Event { ContractExecutionId = exec.Id, StateId = state.Id };
        _db.Events.Add(newEvent);
        await _db.SaveChangesAsync(ct);

        return new ApiResponse { Status = true };
    }
}
