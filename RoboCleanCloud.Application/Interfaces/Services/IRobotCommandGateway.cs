using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RoboCleanCloud.Domain.Enums;

namespace RoboCleanCloud.Application.Interfaces.Services;

public interface IRobotCommandGateway
{
    Task SendCleaningCommandAsync(Guid robotId, Guid sessionId, CleaningMode mode, List<Guid> zoneIds, CancellationToken cancellationToken = default);
    Task SendReturnToBaseCommandAsync(Guid robotId, CancellationToken cancellationToken = default);
    Task SendPauseCommandAsync(Guid robotId, CancellationToken cancellationToken = default);
    Task SendResumeCommandAsync(Guid robotId, CancellationToken cancellationToken = default);
    Task SendStopCommandAsync(Guid robotId, CancellationToken cancellationToken = default);
    Task<bool> TestConnectionAsync(Guid robotId, CancellationToken cancellationToken = default);
}