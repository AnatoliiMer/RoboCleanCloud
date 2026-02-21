using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RoboCleanCloud.Domain.Entities;
using RoboCleanCloud.Domain.Enums;

namespace RoboCleanCloud.Application.Interfaces.Repositories;

public interface ICleaningSessionRepository : IEntityRepository<CleaningSession>  // Изменено на IEntityRepository
{
    Task<IEnumerable<CleaningSession>> GetByRobotIdAsync(Guid robotId, CancellationToken cancellationToken = default);
    Task<IEnumerable<CleaningSession>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<CleaningSession?> GetActiveSessionAsync(Guid robotId, CancellationToken cancellationToken = default);
    Task<IEnumerable<CleaningSession>> GetByStatusAsync(CleaningSessionStatus status, CancellationToken cancellationToken = default);
}