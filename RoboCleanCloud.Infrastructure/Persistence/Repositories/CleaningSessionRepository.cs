using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RoboCleanCloud.Application.Interfaces.Repositories;
using RoboCleanCloud.Domain.Entities;
using RoboCleanCloud.Domain.Enums;

namespace RoboCleanCloud.Infrastructure.Persistence.Repositories;

public class CleaningSessionRepository : EntityRepositoryBase<CleaningSession>, ICleaningSessionRepository
{
    public CleaningSessionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<CleaningSession>> GetByRobotIdAsync(Guid robotId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(cs => cs.RobotId == robotId)
            .OrderByDescending(cs => cs.StartedAt)
            .Include(cs => cs.Errors)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<CleaningSession>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(cs => cs.StartedAt >= from && cs.StartedAt <= to)
            .OrderByDescending(cs => cs.StartedAt)
            .Include(cs => cs.Robot)
            .ToListAsync(cancellationToken);
    }

    public async Task<CleaningSession?> GetActiveSessionAsync(Guid robotId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(cs => cs.RobotId == robotId &&
                        (cs.Status == CleaningSessionStatus.InProgress ||
                         cs.Status == CleaningSessionStatus.Paused))
            .Include(cs => cs.Errors)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<CleaningSession>> GetByStatusAsync(CleaningSessionStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(cs => cs.Status == status)
            .OrderByDescending(cs => cs.StartedAt)
            .Include(cs => cs.Robot)
            .ToListAsync(cancellationToken);
    }
}