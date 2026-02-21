using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RoboCleanCloud.Application.Interfaces.Services;
using RoboCleanCloud.Infrastructure.Configuration;

namespace RoboCleanCloud.Infrastructure.Services;

public class VendorApiClient : IVendorApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<VendorApiClient> _logger;
    private readonly VendorApiSettings _settings;

    public VendorApiClient(
        HttpClient httpClient,
        IOptions<VendorApiSettings> settings,
        ILogger<VendorApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _settings = settings.Value;

        _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
        _httpClient.DefaultRequestHeaders.Add("X-API-Key", _settings.ApiKey);
        _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);
    }

    public async Task<bool> ValidateSerialNumberAsync(string serialNumber, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/robots/validate/{serialNumber}", cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ValidationResponse>(cancellationToken: cancellationToken);
                return result?.IsValid ?? false;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate serial number {SerialNumber}", serialNumber);
            return false;
        }
    }

    public async Task<FirmwareInfo?> GetLatestFirmwareAsync(string model, string currentVersion, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"api/v1/firmware/latest?model={model}&current={currentVersion}",
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<FirmwareInfo>(cancellationToken: cancellationToken);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get firmware info for model {Model}", model);
            return null;
        }
    }

    public async Task SendDiagnosticDataAsync(Guid robotId, object data, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"api/v1/robots/{robotId}/diagnostics",
                data,
                cancellationToken);

            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send diagnostic data for robot {RobotId}", robotId);
            // Не бросаем исключение - диагностика не критична
        }
    }

    public async Task<FirmwareUpdateResult?> RequestFirmwareUpdateAsync(Guid robotId, string firmwareVersion, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new { firmwareVersion };
            var response = await _httpClient.PostAsJsonAsync(
                $"api/v1/robots/{robotId}/firmware/update",
                request,
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<FirmwareUpdateResult>(cancellationToken: cancellationToken);
            }

            return new FirmwareUpdateResult
            {
                Success = false,
                ErrorMessage = $"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to request firmware update for robot {RobotId}", robotId);
            return new FirmwareUpdateResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    private class ValidationResponse
    {
        public bool IsValid { get; set; }
    }
}

