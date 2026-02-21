using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RoboCleanCloud.Domain.Entities;
using RoboCleanCloud.Domain.Enums;

namespace RoboCleanCloud.Application.Interfaces.Repositories;

public interface IMaintenanceItemRepository : IEntityRepository<MaintenanceItem>  // Изменено на IEntityRepository
{
    Task<IEnumerable<MaintenanceItem>> GetByRobotIdAsync(Guid robotId, CancellationToken cancellationToken = default);
    Task<IEnumerable<MaintenanceItem>> GetItemsNeedingMaintenanceAsync(int healthThreshold = 20, CancellationToken cancellationToken = default);
}