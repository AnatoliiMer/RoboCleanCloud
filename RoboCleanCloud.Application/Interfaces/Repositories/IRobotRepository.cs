using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RoboCleanCloud.Domain.Entities;

namespace RoboCleanCloud.Application.Interfaces.Repositories;

public interface IRobotRepository : IAggregateRepository<Robot>
{
    Task<Robot?> GetBySerialNumberAsync(string serialNumber, CancellationToken cancellationToken = default);
    Task<IEnumerable<Robot>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Robot>> GetOnlineRobotsAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsBySerialNumberAsync(string serialNumber, CancellationToken cancellationToken = default);  // ЭТОТ МЕТОД
}