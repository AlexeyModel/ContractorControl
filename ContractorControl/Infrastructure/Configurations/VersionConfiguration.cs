using ContractorControl.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Version = ContractorControl.Domain.Entities.Version;

namespace ContractorControl.Infrastructure.Configurations;

public class VersionConfiguration : IEntityTypeConfiguration<Version>
{
    public void Configure(EntityTypeBuilder<Version> builder)
    {
        builder.ToTable("versions", "contractor_control");
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).UseSerialColumn();
        builder.Property(v => v.VersionNumber).HasMaxLength(100).IsRequired();
        builder.Property(v => v.ReleaseNotes);
        builder.HasIndex(v => v.VersionNumber).IsUnique();

        builder.HasData(
            new Version { Id = 1, VersionNumber = "1.5.0" }
        );
    }
}
