using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoboCleanCloud.Domain.Entities;

namespace RoboCleanCloud.Infrastructure.Persistence.Configurations;

public class RobotConfiguration : IEntityTypeConfiguration<Robot>
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

        builder.Property(r => r.FriendlyName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.Model)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.FirmwareVersion)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.BatteryLevel)
            .IsRequired();

        builder.Property(r => r.ConnectionStatus)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        // Owned types
        builder.OwnsMany(r => r.StatusHistory, history =>
        {
            history.WithOwner().HasForeignKey("RobotId");
            history.Property<int>("Id");
            history.HasKey("Id");
        });

        // Value conversions
        builder.Property(r => r.DustbinLevel)
            .HasDefaultValue(0);

        // Indexes
        builder.HasIndex(r => r.OwnerId);
        builder.HasIndex(r => r.ConnectionStatus);
        builder.HasIndex(r => r.LastSeenAt);
    }
}