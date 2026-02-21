using System;
using System.Collections.Generic;
using RoboCleanCloud.Domain.Enums;

namespace RoboCleanCloud.Application.DTOs;

public class CleaningScheduleDto
{
    public Guid Id { get; set; }
    public Guid RobotId { get; set; }
    public string RobotName { get; set; } = null!;
    public string CronExpression { get; set; } = null!;
    public CleaningMode Mode { get; set; }
    public List<Guid> ZoneIds { get; set; } = new();
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastTriggeredAt { get; set; }
    public DateTime? NextExecution { get; set; }
    public string TimeZone { get; set; } = "UTC";
    public int? QuietHoursStart { get; set; }
    public int? QuietHoursEnd { get; set; }
}

public class CreateScheduleDto
{
    public Guid RobotId { get; set; }
    public string CronExpression { get; set; } = null!;
    public CleaningMode Mode { get; set; }
    public List<Guid> ZoneIds { get; set; } = new();
    public string TimeZone { get; set; } = "UTC";
    public int? QuietHoursStart { get; set; }
    public int? QuietHoursEnd { get; set; }
}

public class UpdateScheduleDto
{
    public string? CronExpression { get; set; }
    public CleaningMode? Mode { get; set; }
    public List<Guid>? ZoneIds { get; set; }
    public bool? IsActive { get; set; }
    public int? QuietHoursStart { get; set; }
    public int? QuietHoursEnd { get; set; }
}