using ContractorControl.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContractorControl.Infrastructure.Configurations;

public class ContractConfiguration : IEntityTypeConfiguration<Contract>
{
    public void Configure(EntityTypeBuilder<Contract> builder)
    {
        builder.ToTable("contracts", "contractor_control");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).UseSerialColumn();
        builder.Property(c => c.Code).HasMaxLength(500).IsRequired();
        builder.Property(c => c.CreateAt).HasDefaultValueSql("now()").IsRequired();
        builder.Property(c => c.UpdateAt);
        builder.Property(c => c.DeleteAt);
        builder.HasIndex(c => c.Code);
        builder.HasIndex(c => new { c.Code, c.DeleteAt }).IsUnique();
    }
}
