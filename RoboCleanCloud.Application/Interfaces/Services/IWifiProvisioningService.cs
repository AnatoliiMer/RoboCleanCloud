using System;
using System.Threading;
using System.Threading.Tasks;

namespace RoboCleanCloud.Application.Interfaces.Services;

public interface IWifiProvisioningService
{
    Task<bool> ProvisionRobotAsync(Guid robotId, string ssid, string password, CancellationToken cancellationToken = default);
    Task<bool> TestConnectionAsync(Guid robotId, CancellationToken cancellationToken = default);
    Task<string?> GetConnectionStatusAsync(Guid robotId, CancellationToken cancellationToken = default);
}