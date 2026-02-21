using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RoboCleanCloud.Application.Interfaces.Repositories;
using RoboCleanCloud.Domain.Entities;

namespace RoboCleanCloud.Infrastructure.Persistence.Repositories;

public class MaintenanceItemRepository : EntityRepositoryBase<MaintenanceItem>, IMaintenanceItemRepository
{
    public MaintenanceItemRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<MaintenanceItem>> GetByRobotIdAsync(Guid robotId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(mi => mi.RobotId == robotId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<MaintenanceItem>> GetItemsNeedingMaintenanceAsync(int healthThreshold = 20, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(mi => mi.CurrentHealth <= healthThreshold)
            .Include(mi => mi.Robot)
            .ToListAsync(cancellationToken);
    }
}