using ContractorControl.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContractorControl.Infrastructure.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.ToTable("events", "contractor_control");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).UseSerialColumn();
        builder.Property(e => e.CreateAt).HasDefaultValueSql("now()").IsRequired();

        builder.HasIndex(e => e.ContractExecutionId);
        builder.HasIndex(e => e.StateId);
        builder.HasIndex(s => new { s.ContractExecutionId, s.StateId, s.DeleteAt }).IsUnique();
    }
}
