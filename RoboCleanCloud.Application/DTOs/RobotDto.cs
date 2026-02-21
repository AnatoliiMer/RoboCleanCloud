using System;
using System.Collections.Generic;
using RoboCleanCloud.Domain.Enums;

namespace RoboCleanCloud.Application.DTOs;

public class RobotDto
{
    public Guid Id { get; set; }
    public string SerialNumber { get; set; } = null!;
    public string Model { get; set; } = null!;
    public string FriendlyName { get; set; } = null!;
    public Guid OwnerId { get; set; }
    public ConnectionStatus ConnectionStatus { get; set; }
    public int BatteryLevel { get; set; }
    public string FirmwareVersion { get; set; } = null!;
    public DateTime RegisteredAt { get; set; }
    public DateTime? LastSeenAt { get; set; }
    public int DustbinLevel { get; set; }
}

public class RobotDetailsDto : RobotDto
{
    public List<RobotStatusHistoryDto> StatusHistory { get; set; } = new();
    public List<MaintenanceItemDto> MaintenanceItems { get; set; } = new();
    public List<CleaningSessionDto> RecentSessions { get; set; } = new();
}

public class RobotStatusHistoryDto
{
    public Guid Id { get; set; }
    public ConnectionStatus PreviousStatus { get; set; }
    public ConnectionStatus NewStatus { get; set; }
    public string? Reason { get; set; }
    public DateTime Timestamp { get; set; }
}

public class MaintenanceItemDto
{
    public Guid Id { get; set; }
    public ItemType Type { get; set; }
    public int CurrentHealth { get; set; }
    public DateTime LastReplacedAt { get; set; }
    public int EstimatedDaysLeft { get; set; }
}

public class CreateRobotDto
{
    public string SerialNumber { get; set; } = null!;
    public string Model { get; set; } = null!;
    public string FriendlyName { get; set; } = null!;
    public string WifiSsid { get; set; } = null!;
    public string WifiPassword { get; set; } = null!;
}

public class UpdateRobotDto
{
    public string? FriendlyName { get; set; }
    public string? FirmwareVersion { get; set; }
}