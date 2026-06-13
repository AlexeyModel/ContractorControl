using ContractorControl.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Version = ContractorControl.Domain.Entities.Version;

namespace ContractorControl.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Contract> Contracts { get; }
    DbSet<State> States { get; }
    DbSet<StateType> StateTypes { get; }
    DbSet<Dag> Dags { get; }
    DbSet<ContractExecution> ContractExecutions { get; }
    DbSet<Event> Events { get; }
    DbSet<Version> Versions { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
