using ContractorControl.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContractorControl.Infrastructure.Configurations;

public class StateConfiguration : IEntityTypeConfiguration<State>
{
    public void Configure(EntityTypeBuilder<State> builder)
    {
        builder.ToTable("states", "contractor_control");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).UseSerialColumn();
        builder.Property(s => s.Name).HasMaxLength(500).IsRequired();
        builder.Property(e => e.Description);
        builder.Property(e => e.ContractId);
        builder.Property(e => e.StateTypeId);
        builder.Property(s => s.CreateAt).HasDefaultValueSql("now()").IsRequired();
        builder.Property(e => e.UpdateAt);
        builder.Property(e => e.DeleteAt);

        builder.HasIndex(s => s.Name);
        builder.HasIndex(s => s.ContractId);
        builder.HasIndex(s => new { s.Name, s.StateTypeId, s.ContractId, s.DeleteAt }).IsUnique();

        builder.HasData(
            new State { Id = 1, Name = "FINISHED", ContractId = 0, StateTypeId = 1 }, // Reserved
            new State { Id = 2, Name = "FAILED", ContractId = 0, StateTypeId = 1 }, // Reserved
            new State { Id = 3, Name = "QUEUE", ContractId = 0, StateTypeId = 1 } // Reserved
        );
    }
}
