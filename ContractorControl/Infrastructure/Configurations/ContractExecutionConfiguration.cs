using ContractorControl.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContractorControl.Infrastructure.Configurations;

public class ContractExecutionConfiguration : IEntityTypeConfiguration<ContractExecution>
{
    public void Configure(EntityTypeBuilder<ContractExecution> builder)
    {
        builder.ToTable("contract_execution", "contractor_control");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).UseSerialColumn();
        builder.Property(e => e.ContractId);
        builder.Property(e => e.InstanceContractExecution).IsRequired();
        builder.Property(e => e.CreateAt).HasDefaultValueSql("now()").IsRequired();
        builder.Property(e => e.UpdateAt);
        builder.Property(e => e.DeleteAt);
        builder.HasIndex(e => e.ContractId);
        builder.HasIndex(e => e.InstanceContractExecution).IsUnique();
    }
}
