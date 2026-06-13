using ContractorControl.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContractorControl.Infrastructure.Configurations;

public class DagConfiguration : IEntityTypeConfiguration<Dag>
{
    public void Configure(EntityTypeBuilder<Dag> builder)
    {
        builder.ToTable("dags", "contractor_control");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).UseSerialColumn();
        builder.Property(e => e.StateDestinationId);
        builder.Property(e => e.StateSourceId);
        builder.Property(e => e.UpdateAt);
        builder.Property(e => e.DeleteAt);
        builder.Property(d => d.CreateAt).HasDefaultValueSql("now()").IsRequired();
        builder.HasIndex(d => new { d.StateSourceId, d.StateDestinationId, d.DeleteAt }).IsUnique();
    }
}
