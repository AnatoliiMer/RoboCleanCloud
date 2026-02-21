using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using RoboCleanCloud.Application.Interfaces.Repositories;
using StackExchange.Redis;

namespace RoboCleanCloud.Api.HealthChecks;

public class RobotConnectivityHealthCheck : IHealthCheck
{
    private readonly IRobotRepository _robotRepository;
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RobotConnectivityHealthCheck> _logger;

    public RobotConnectivityHealthCheck(
        IRobotRepository robotRepository,
        IConnectionMultiplexer redis,
        ILogger<RobotConnectivityHealthCheck> logger)
    {
        _robotRepository = robotRepository;
        _redis = redis;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // 1. Проверяем общее количество подключенных роботов
            var onlineRobots = await _robotRepository.GetOnlineRobotsAsync(cancellationToken);
            var onlineCount = onlineRobots?.Count() ?? 0;

            // 2. Проверяем среднюю latency команд
            var db = _redis.GetDatabase();
            var avgLatency = await db.StringGetAsync("metrics:command_avg_latency");

            var data = new Dictionary<string, object>
            {
                { "online_robots", onlineCount },
                { "avg_command_latency_ms", avgLatency.TryParse(out double val) ? val : 0 },
                { "timestamp", DateTime.UtcNow }
            };

            // 3. Определяем статус
            if (onlineCount > 1000)
            {
                return new HealthCheckResult(
                    HealthStatus.Healthy,
                    description: "High connectivity",
                    data: data);
            }
            else if (onlineCount > 100)
            {
                return new HealthCheckResult(
                    HealthStatus.Degraded,
                    description: "Normal connectivity",
                    data: data);
            }
            else if (onlineCount > 0)
            {
                return new HealthCheckResult(
                    HealthStatus.Degraded,
                    description: "Low connectivity",
                    data: data);
            }
            else
            {
                return new HealthCheckResult(
                    HealthStatus.Unhealthy,
                    description: "No robots connected",
                    data: data);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Robot connectivity health check failed");
            return new HealthCheckResult(
                HealthStatus.Unhealthy,
                description: "Failed to check robot connectivity",
                exception: ex);
        }
    }
}