using ContractorControl.Application.Interfaces;
using ContractorControl.Domain.Common;
using ContractorControl.Domain.Entities;
using ContractorControl.Infrastructure.Configurations;
using ContractorControl.Infrastructure.Interceptors;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Version = ContractorControl.Domain.Entities.Version;

namespace ContractorControl.Infrastructure.Data;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly AuditableEntityInterceptor _auditableInterceptor;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options,
        AuditableEntityInterceptor auditableInterceptor) : base(options)
    {
        _auditableInterceptor = auditableInterceptor;
    }

    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<State> States => Set<State>();
    public DbSet<StateType> StateTypes => Set<StateType>();
    public DbSet<Dag> Dags => Set<Dag>();
    public DbSet<ContractExecution> ContractExecutions => Set<ContractExecution>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Version> Versions => Set<Version>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_auditableInterceptor);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("contractor_control");

        modelBuilder.ApplyConfiguration(new ContractConfiguration());
        modelBuilder.ApplyConfiguration(new ContractExecutionConfiguration());
        modelBuilder.ApplyConfiguration(new DagConfiguration());
        modelBuilder.ApplyConfiguration(new EventConfiguration());
        modelBuilder.ApplyConfiguration(new StateConfiguration());
        modelBuilder.ApplyConfiguration(new StateTypeConfiguration());
        modelBuilder.ApplyConfiguration(new VersionConfiguration());

        // Global Query Filter для soft-delete
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(IAuditable).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType);
                var property = Expression.Property(parameter, nameof(IAuditable.DeleteAt));

                // Явно указываем тип null для стабильной трансляции в SQL
                var nullConstant = Expression.Constant(null, typeof(DateTime?));
                var nullCheck = Expression.Equal(property, nullConstant);
                var filter = Expression.Lambda(nullCheck, parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
            }
        }

        base.OnModelCreating(modelBuilder);
    }
}
