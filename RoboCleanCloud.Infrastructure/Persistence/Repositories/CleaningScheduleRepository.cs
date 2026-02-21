using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RoboCleanCloud.Application.Interfaces.Repositories;
using RoboCleanCloud.Domain.Entities;

namespace RoboCleanCloud.Infrastructure.Persistence.Repositories;

public class CleaningScheduleRepository : AggregateRepositoryBase<CleaningSchedule>, ICleaningScheduleRepository
{
    public CleaningScheduleRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<CleaningSchedule>> GetByRobotIdAsync(Guid robotId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(cs => cs.RobotId == robotId)
            .OrderBy(cs => cs.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<CleaningSchedule>> GetActiveSchedulesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(cs => cs.IsActive)
            .Include(cs => cs.Robot)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<CleaningSchedule>> GetSchedulesDueForExecutionAsync(DateTime currentTime, CancellationToken cancellationToken = default)
    {
        // Здесь сложная логика с cron выражениями - пока возвращаем все активные
        // В реальном проекте нужно вычислять следующие выполнения
        return await _dbSet
            .Where(cs => cs.IsActive)
            .Include(cs => cs.Robot)
            .ToListAsync(cancellationToken);
    }
}