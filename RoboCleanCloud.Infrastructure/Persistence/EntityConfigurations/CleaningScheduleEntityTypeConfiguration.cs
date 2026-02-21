using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoboCleanCloud.Domain.Entities;

namespace RoboCleanCloud.Infrastructure.Persistence.EntityConfigurations;

public class CleaningScheduleEntityTypeConfiguration : IEntityTypeConfiguration<CleaningSchedule>
{
    public void Configure(EntityTypeBuilder<CleaningSchedule> builder)
    {
        builder.ToTable("CleaningSchedules");

        builder.HasKey(cs => cs.Id);

        builder.Property(cs => cs.CronExpression)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(cs => cs.Mode)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(cs => cs.ZoneIds)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(cs => cs.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(cs => cs.CreatedAt)
            .IsRequired();

        builder.Property(cs => cs.LastTriggeredAt)
            .IsRequired(false);

        builder.Property(cs => cs.TimeZone)
            .IsRequired()
            .HasMaxLength(100)
            .HasDefaultValue("UTC");

        builder.Property(cs => cs.QuietHoursStart)
            .IsRequired(false);

        builder.Property(cs => cs.QuietHoursEnd)
            .IsRequired(false);

        // Индексы
        builder.HasIndex(cs => cs.RobotId);
        builder.HasIndex(cs => cs.IsActive);
        builder.HasIndex(cs => cs.LastTriggeredAt);

        // Relationships
        builder.HasOne(cs => cs.Robot)
            .WithMany()
            .HasForeignKey(cs => cs.RobotId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}