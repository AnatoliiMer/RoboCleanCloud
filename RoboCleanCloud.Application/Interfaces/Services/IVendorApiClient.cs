using System;
using System.Threading;
using System.Threading.Tasks;

namespace RoboCleanCloud.Application.Interfaces.Services;

public interface IVendorApiClient
{
    Task<bool> ValidateSerialNumberAsync(string serialNumber, CancellationToken cancellationToken = default);
    Task<FirmwareInfo?> GetLatestFirmwareAsync(string model, string currentVersion, CancellationToken cancellationToken = default);
    Task SendDiagnosticDataAsync(Guid robotId, object data, CancellationToken cancellationToken = default);
    Task<FirmwareUpdateResult?> RequestFirmwareUpdateAsync(Guid robotId, string firmwareVersion, CancellationToken cancellationToken = default);
}

public class FirmwareInfo
{
    public string Version { get; set; } = null!;
    public string DownloadUrl { get; set; } = null!;
    public string Changelog { get; set; } = null!;
    public long Size { get; set; }
    public string Checksum { get; set; } = null!;
    public bool IsCritical { get; set; }
    public DateTime ReleaseDate { get; set; }
}

public class FirmwareUpdateResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? JobId { get; set; }
}