using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RoboCleanCloud.Infrastructure.Configuration;

namespace RoboCleanCloud.Api.HealthChecks;

public class VendorApiHealthCheck : IHealthCheck
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<VendorApiHealthCheck> _logger;
    private readonly VendorApiSettings _settings;

    public VendorApiHealthCheck(
        HttpClient httpClient,
        IOptions<VendorApiSettings> settings,
        ILogger<VendorApiHealthCheck> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _settings = settings.Value;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var startTime = DateTime.UtcNow;

            // Простой ping запрос
            var response = await _httpClient.GetAsync(
                $"{_settings.BaseUrl}/health",
                cancellationToken);

            var latency = DateTime.UtcNow - startTime;

            var data = new Dictionary<string, object>
            {
                { "latency_ms", latency.TotalMilliseconds },
                { "status_code", (int)response.StatusCode },
                { "base_url", _settings.BaseUrl }
            };

            if (response.IsSuccessStatusCode)
            {
                return HealthCheckResult.Healthy("Vendor API is reachable", data);
            }

            return HealthCheckResult.Degraded(
                $"Vendor API returned {response.StatusCode}",
                data: data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Vendor API health check failed");
            return HealthCheckResult.Unhealthy(
                "Vendor API is unreachable",
                ex);
        }
    }
}