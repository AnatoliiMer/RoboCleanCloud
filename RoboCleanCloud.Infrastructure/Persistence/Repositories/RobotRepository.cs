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

public class RobotRepository : AggregateRepositoryBase<Robot>, IRobotRepository
{
    public RobotRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Robot?> GetBySerialNumberAsync(string serialNumber, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.StatusHistory)
            .Include(r => r.MaintenanceItems)
            .FirstOrDefaultAsync(r => r.SerialNumber == serialNumber, cancellationToken);
    }

    public async Task<IEnumerable<Robot>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(r => r.OwnerId == ownerId)
            .Include(r => r.StatusHistory.OrderByDescending(sh => sh.Timestamp).Take(10))
            .Include(r => r.MaintenanceItems)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Robot>> GetOnlineRobotsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(r => r.ConnectionStatus == ConnectionStatus.Online ||
                       r.ConnectionStatus == ConnectionStatus.Busy)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsBySerialNumberAsync(string serialNumber, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(r => r.SerialNumber == serialNumber, cancellationToken);
    }
}