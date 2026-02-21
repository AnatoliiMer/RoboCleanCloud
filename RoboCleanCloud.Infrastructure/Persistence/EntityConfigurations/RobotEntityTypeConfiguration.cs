using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoboCleanCloud.Domain.Entities;

namespace RoboCleanCloud.Infrastructure.Persistence.EntityConfigurations;

public class RobotEntityTypeConfiguration : IEntityTypeConfiguration<Robot>
{
    public void Configure(EntityTypeBuilder<Robot> builder)
    {
        builder.ToTable("Robots");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.SerialNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(r => r.SerialNumber)
            .IsUnique();

        builder.Property(r => r.Model)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.FriendlyName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.FirmwareVersion)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.ConnectionStatus)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(r => r.OwnerId)
            .IsRequired();

        builder.Property(r => r.BatteryLevel)
            .IsRequired();

        builder.Property(r => r.DustbinLevel)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(r => r.RegisteredAt)
            .IsRequired();

        builder.Property(r => r.LastSeenAt)
            .IsRequired(false);

        // Индексы для частых запросов
        builder.HasIndex(r => r.OwnerId);
        builder.HasIndex(r => r.ConnectionStatus);
        builder.HasIndex(r => r.LastSeenAt);

        // Relationships
        builder.HasMany(r => r.StatusHistory)
            .WithOne(sh => sh.Robot)
            .HasForeignKey(sh => sh.RobotId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.MaintenanceItems)
            .WithOne(mi => mi.Robot)
            .HasForeignKey(mi => mi.RobotId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.CleaningSessions)
            .WithOne(cs => cs.Robot)
            .HasForeignKey(cs => cs.RobotId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}