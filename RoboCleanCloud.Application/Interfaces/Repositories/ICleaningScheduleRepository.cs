using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RoboCleanCloud.Domain.Entities;

namespace RoboCleanCloud.Application.Interfaces.Repositories;

public interface ICleaningScheduleRepository : IAggregateRepository<CleaningSchedule>  // CleaningSchedule - AggregateRoot
{
    Task<IEnumerable<CleaningSchedule>> GetByRobotIdAsync(Guid robotId, CancellationToken cancellationToken = default);
    Task<IEnumerable<CleaningSchedule>> GetActiveSchedulesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<CleaningSchedule>> GetSchedulesDueForExecutionAsync(DateTime currentTime, CancellationToken cancellationToken = default);
}