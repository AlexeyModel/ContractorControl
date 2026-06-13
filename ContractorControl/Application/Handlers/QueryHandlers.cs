using ContractorControl.Application.DTOs;
using ContractorControl.Application.Interfaces;
using ContractorControl.Application.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ContractorControl.Application.Handlers;

public class CheckStateIsSetHandler : IRequestHandler<CheckStateIsSetQuery, ApiResponse>
{
    private readonly IApplicationDbContext _db;
    public CheckStateIsSetHandler(IApplicationDbContext db) => _db = db;

    public async Task<ApiResponse> Handle(CheckStateIsSetQuery request, CancellationToken ct)
    {
        var dto = request.Dto;
        var exec = await _db.ContractExecutions.FirstOrDefaultAsync(e => e.InstanceContractExecution == dto.InstanceContractExecution, ct);
        if (exec == null) return new ApiResponse { Status = false, Message = "Execution not found" };

        var stateType = await _db.StateTypes.FirstOrDefaultAsync(st => st.Type == dto.StateType, ct);
        if (stateType == null) return new ApiResponse { Status = false, Message = "StateType not found" };

        var state = await _db.States.FirstOrDefaultAsync(s => s.Name == dto.StateName && s.ContractId == exec.ContractId && s.StateTypeId == stateType.Id, ct);
        if (state == null) return new ApiResponse { Status = false, Message = "State not found" };

        var exists = await _db.Events
            .AnyAsync(e => e.ContractExecutionId == exec.Id && e.StateId == state.Id, ct);

        return exists 
            ? new ApiResponse { Status = true, Message = "состояние установлено" } 
            : new ApiResponse { Status = false, Message = "состояние не установлено" };
    }
}

public class CheckStateToSetHandler : IRequestHandler<CheckStateToSetQuery, ApiResponse>
{
    private readonly IApplicationDbContext _db;
    public CheckStateToSetHandler(IApplicationDbContext db) => _db = db;

    public async Task<ApiResponse> Handle(CheckStateToSetQuery request, CancellationToken ct)
    {
        var dto = request.Dto;
        var conn = ((DbContext)_db).Database.GetDbConnection();
        await conn.OpenAsync(ct);
        
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "SELECT contractor_control.check_state_to_set(@p1, @p2, @p3)";
            var p1 = cmd.CreateParameter(); p1.ParameterName = "p1"; p1.Value = dto.InstanceContractExecution; cmd.Parameters.Add(p1);
            var p2 = cmd.CreateParameter(); p2.ParameterName = "p2"; p2.Value = dto.StateName; cmd.Parameters.Add(p2);
            var p3 = cmd.CreateParameter(); p3.ParameterName = "p3"; p3.Value = dto.StateType; cmd.Parameters.Add(p3);

            var result = await cmd.ExecuteScalarAsync(ct);
            bool canSet = (bool)result!;

            return canSet 
                ? new ApiResponse { Status = true, Message = "установка возможна" } 
                : new ApiResponse { Status = false, Message = "установка невозможна" };
        }
    }
}

public class CheckIsFinishedHandler : IRequestHandler<CheckIsFinishedQuery, ApiResponse>
{
    private readonly IApplicationDbContext _db;
    public CheckIsFinishedHandler(IApplicationDbContext db) => _db = db;

    public async Task<ApiResponse> Handle(CheckIsFinishedQuery request, CancellationToken ct)
    {
        var dto = request.Dto;
        var conn = ((DbContext)_db).Database.GetDbConnection();
        await conn.OpenAsync(ct);

        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "SELECT contractor_control.check_is_finished(@p1)";
            var p1 = cmd.CreateParameter(); p1.ParameterName = "p1"; p1.Value = dto.InstanceContractExecution; cmd.Parameters.Add(p1);
            
            var result = await cmd.ExecuteScalarAsync(ct);
            bool isFinished = (bool)result!;

            return isFinished 
                ? new ApiResponse { Status = true, Message = "контракт исполнен" } 
                : new ApiResponse { Status = false, Message = "контракт ещё не исполнен" };
        }
    }
}

public class SetFinishedHandler : IRequestHandler<SetFinishedQuery, ApiResponse>
{
    private readonly IApplicationDbContext _db;
    public SetFinishedHandler(IApplicationDbContext db) => _db = db;

    public async Task<ApiResponse> Handle(SetFinishedQuery request, CancellationToken ct)
    {
        var dto = request.Dto;
        var conn = ((DbContext)_db).Database.GetDbConnection();
        await conn.OpenAsync(ct);

        using (var cmd = conn.CreateCommand())
        {
            var exec = await _db.ContractExecutions.FirstOrDefaultAsync(e => e.InstanceContractExecution == dto.InstanceContractExecution, ct);

            if (exec != null)
            {
                cmd.CommandText = $"INSERT INTO contractor_control.events(contract_execution_id, state_id, state_type_id, create_at) VALUES({exec.Id}, 1, 1, now());";
                var result = await cmd.ExecuteNonQueryAsync(ct);

                return new ApiResponse { Status = true, Message = "Попытка завершения контракта" };
            }

            return new ApiResponse { Status = false, Message = "Неудачная попытка завершения контракта" };
        }
    }
}

public class GetEventsHandler : IRequestHandler<GetEventsQuery, ApiResponse>
{
    private readonly IApplicationDbContext _db;
    public GetEventsHandler(IApplicationDbContext db) => _db = db;

    public async Task<ApiResponse> Handle(GetEventsQuery request, CancellationToken ct)
    {
        var dto = request.Dto;
        var events = await _db.Events
                                .Where(e => e.ContractExecutionRef!.InstanceContractExecution == dto.InstanceContractExecution)
                                .Select(e => new EventResponseDto
                                {
                                    InstanceContractExecution = e.ContractExecutionRef!.InstanceContractExecution,
                                    CreateAt = e.CreateAt,
                                    UpdateAt = e.UpdateAt,
                                    DeleteAt = e.DeleteAt,
                                    ContractId = e.StateRef!.ContractId,
                                    StateName = e.StateRef.Name,
                                    StateType = e.StateRef.StateTypeRef!.Type
                                })
                                .ToListAsync(ct);

        return new ApiResponse { Status = true, Data = new { Events = events.Select(e => new { Event = e }).ToList() } };
    }
}
