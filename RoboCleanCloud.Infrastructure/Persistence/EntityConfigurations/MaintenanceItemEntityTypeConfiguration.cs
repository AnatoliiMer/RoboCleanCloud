using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoboCleanCloud.Domain.Entities;

namespace RoboCleanCloud.Infrastructure.Persistence.EntityConfigurations;

public class MaintenanceItemEntityTypeConfiguration : IEntityTypeConfiguration<MaintenanceItem>
{
    public void Configure(EntityTypeBuilder<MaintenanceItem> builder)
    {
        builder.ToTable("MaintenanceItems");

        builder.HasKey(mi => mi.Id);

        builder.Property(mi => mi.Type)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(mi => mi.CurrentHealth)
            .IsRequired();

        builder.Property(mi => mi.LastReplacedAt)
            .IsRequired();

        builder.Property(mi => mi.EstimatedDaysLeft)
            .IsRequired();

        // Индексы
        builder.HasIndex(mi => mi.RobotId);
        builder.HasIndex(mi => mi.Type);
        builder.HasIndex(mi => mi.CurrentHealth);

        // Relationships
        builder.HasOne(mi => mi.Robot)
            .WithMany(r => r.MaintenanceItems)
            .HasForeignKey(mi => mi.RobotId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}