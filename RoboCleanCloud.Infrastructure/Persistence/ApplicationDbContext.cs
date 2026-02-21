using Microsoft.EntityFrameworkCore;
using RoboCleanCloud.Domain.Entities;
using RoboCleanCloud.Infrastructure.Persistence.EntityConfigurations;

namespace RoboCleanCloud.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSets для всех сущностей
    public DbSet<Robot> Robots { get; set; }
    public DbSet<CleaningSession> CleaningSessions { get; set; }
    public DbSet<CleaningSchedule> CleaningSchedules { get; set; }
    public DbSet<MaintenanceItem> MaintenanceItems { get; set; }
    public DbSet<RobotStatusHistory> RobotStatusHistories { get; set; }
    public DbSet<CleaningError> CleaningErrors { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new RobotEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new CleaningSessionEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new CleaningScheduleEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new MaintenanceItemEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new RobotStatusHistoryEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new CleaningErrorEntityTypeConfiguration());

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Аудит изменений
        UpdateAuditFields();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateAuditFields()
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is not null &&
                       (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            // Здесь можно добавить автоматическое заполнение полей аудита
            // например CreatedAt, UpdatedAt если они есть в сущностях
        }
    }
}