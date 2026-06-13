using ContractorControl.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContractorControl.Infrastructure.Configurations;

public class StateTypeConfiguration : IEntityTypeConfiguration<StateType>
{
    public void Configure(EntityTypeBuilder<StateType> builder)
    {
        builder.ToTable("state_type", "contractor_control");
        builder.HasKey(st => st.Id);
        builder.Property(st => st.Id).UseSerialColumn();
        builder.Property(st => st.Type).HasMaxLength(200).IsRequired();
        builder.Property(st => st.CreateAt).HasDefaultValueSql("now()").IsRequired();
        builder.HasIndex(st => new { st.Type, st.DeleteAt }).IsUnique();

        builder.HasData(
            new StateType { Id = 1, Type = "GLOBAL" }, // Reserved
            new StateType { Id = 2, Type = "RUN" }, // Reserved
            new StateType { Id = 3, Type = "SUCCESS" }, // Reserved
            new StateType { Id = 4, Type = "FAILED" } // Reserved
        );
    }
}
