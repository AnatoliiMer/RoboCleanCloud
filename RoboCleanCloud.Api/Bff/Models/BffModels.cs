using RoboCleanCloud.Application.DTOs;

namespace RoboCleanCloud.Api.Bff.Models;

public class MobileDashboardResponse
{
    public List<MobileRobotSummary> Robots { get; set; } = new();
    public List<MobileScheduleSummary> Schedules { get; set; } = new();
    public List<NotificationDto> Notifications { get; set; } = new();
    public DashboardSummary Summary { get; set; } = new();
}

public class MobileRobotSummary
{
    public Guid RobotId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ConnectionStatus { get; set; } = string.Empty;
    public int BatteryLevel { get; set; }
    public int DustbinLevel { get; set; }
    public bool IsCleaning { get; set; }
    public CleaningSessionDto? CurrentSession { get; set; }
}

public class MobileScheduleSummary
{
    public Guid ScheduleId { get; set; }
    public string RobotName { get; set; } = string.Empty;
    public string? Time { get; set; }
    public string? Days { get; set; }
    public bool IsActive { get; set; }
}

public class NotificationDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public bool IsRead { get; set; }
}

public class DashboardSummary
{
    public int TotalRobots { get; set; }
    public int ActiveCleanings { get; set; }
    public int TotalSchedules { get; set; }
    public int PendingMaintenance { get; set; }
}

public class RobotStatusSummary
{
    public Guid RobotId { get; set; }
    public string Status { get; set; } = string.Empty;
    public int BatteryLevel { get; set; }
    public bool IsCleaning { get; set; }
    public int CleaningProgress { get; set; }
    public int? EstimatedTimeRemaining { get; set; }
    public DateTime LastUpdateTime { get; set; }
}