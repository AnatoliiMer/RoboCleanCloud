using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoboCleanCloud.Domain.Entities;

namespace RoboCleanCloud.Infrastructure.Persistence.EntityConfigurations;

public class CleaningSessionEntityTypeConfiguration : IEntityTypeConfiguration<CleaningSession>
{
    public void Configure(EntityTypeBuilder<CleaningSession> builder)
    {
        builder.ToTable("CleaningSessions");

        builder.HasKey(cs => cs.Id);

        builder.Property(cs => cs.Mode)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(cs => cs.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(cs => cs.ZoneIds)
            .HasColumnType("jsonb")  // PostgreSQL JSONB тип
            .IsRequired();

        builder.Property(cs => cs.StartedAt)
            .IsRequired();

        builder.Property(cs => cs.FinishedAt)
            .IsRequired(false);

        builder.Property(cs => cs.AreaCleaned)
            .IsRequired(false)
            .HasPrecision(10, 2);

        builder.Property(cs => cs.EnergyConsumed)
            .IsRequired(false)
            .HasPrecision(10, 2);

        // Индексы
        builder.HasIndex(cs => cs.RobotId);
        builder.HasIndex(cs => cs.Status);
        builder.HasIndex(cs => cs.StartedAt);

        // Relationships
        builder.HasOne(cs => cs.Robot)
            .WithMany(r => r.CleaningSessions)
            .HasForeignKey(cs => cs.RobotId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cs => cs.Schedule)
            .WithMany()
            .HasForeignKey(cs => cs.ScheduleId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(cs => cs.Errors)
            .WithOne(e => e.Session)
            .HasForeignKey(e => e.SessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}