using Microsoft.Extensions.Logging;
using RoboCleanCloud.Application.Interfaces.Services;
using RoboCleanCloud.Domain.Enums;

namespace RoboCleanCloud.Infrastructure.Services;

public class RobotCommandGatewayStub : IRobotCommandGateway
{
    private readonly ILogger<RobotCommandGatewayStub> _logger;

    public RobotCommandGatewayStub(ILogger<RobotCommandGatewayStub> logger)
    {
        _logger = logger;
    }

    public Task SendCleaningCommandAsync(Guid robotId, Guid sessionId, CleaningMode mode, List<Guid> zoneIds, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("STUB: Cleaning command to robot {RobotId}", robotId);
        return Task.CompletedTask;
    }

    public Task SendReturnToBaseCommandAsync(Guid robotId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("STUB: Return to base command to robot {RobotId}", robotId);
        return Task.CompletedTask;
    }

    public Task SendPauseCommandAsync(Guid robotId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("STUB: Pause command to robot {RobotId}", robotId);
        return Task.CompletedTask;
    }

    public Task SendResumeCommandAsync(Guid robotId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("STUB: Resume command to robot {RobotId}", robotId);
        return Task.CompletedTask;
    }

    public Task SendStopCommandAsync(Guid robotId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("STUB: Stop command to robot {RobotId}", robotId);
        return Task.CompletedTask;
    }

    public Task<bool> TestConnectionAsync(Guid robotId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }
}