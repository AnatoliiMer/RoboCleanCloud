using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoboCleanCloud.Domain.Entities;

namespace RoboCleanCloud.Infrastructure.Persistence.EntityConfigurations;

public class RobotStatusHistoryEntityTypeConfiguration : IEntityTypeConfiguration<RobotStatusHistory>
{
    public void Configure(EntityTypeBuilder<RobotStatusHistory> builder)
    {
        builder.ToTable("RobotStatusHistories");

        builder.HasKey(rsh => rsh.Id);

        builder.Property(rsh => rsh.PreviousStatus)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(rsh => rsh.NewStatus)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(rsh => rsh.Reason)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(rsh => rsh.Timestamp)
            .IsRequired();

        // Индексы
        builder.HasIndex(rsh => rsh.RobotId);
        builder.HasIndex(rsh => rsh.Timestamp);

        // Relationships
        builder.HasOne(rsh => rsh.Robot)
            .WithMany(r => r.StatusHistory)
            .HasForeignKey(rsh => rsh.RobotId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}