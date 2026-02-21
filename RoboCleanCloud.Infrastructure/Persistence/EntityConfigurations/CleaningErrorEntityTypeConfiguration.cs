using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoboCleanCloud.Domain.Entities;

namespace RoboCleanCloud.Infrastructure.Persistence.EntityConfigurations;

public class CleaningErrorEntityTypeConfiguration : IEntityTypeConfiguration<CleaningError>
{
    public void Configure(EntityTypeBuilder<CleaningError> builder)
    {
        builder.ToTable("CleaningErrors");

        builder.HasKey(ce => ce.Id);

        builder.Property(ce => ce.ErrorCode)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(ce => ce.Message)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(ce => ce.Timestamp)
            .IsRequired();

        builder.Property(ce => ce.IsResolved)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(ce => ce.Resolution)
            .HasMaxLength(500)
            .IsRequired(false);

        // Индексы
        builder.HasIndex(ce => ce.SessionId);
        builder.HasIndex(ce => ce.ErrorCode);
        builder.HasIndex(ce => ce.Timestamp);

        // Relationships
        builder.HasOne(ce => ce.Session)
            .WithMany(cs => cs.Errors)
            .HasForeignKey(ce => ce.SessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}