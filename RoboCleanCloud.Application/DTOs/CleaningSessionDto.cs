using System;
using System.Collections.Generic;
using RoboCleanCloud.Domain.Enums;

namespace RoboCleanCloud.Application.DTOs;

public class CleaningSessionDto
{
    public Guid Id { get; set; }
    public Guid RobotId { get; set; }
    public string RobotName { get; set; } = null!;
    public CleaningMode Mode { get; set; }
    public CleaningSessionStatus Status { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public double? AreaCleaned { get; set; }
    public double? EnergyConsumed { get; set; }
    public int Progress { get; set; }
    public int? EstimatedRemainingMinutes { get; set; }
}

public class CleaningSessionDetailsDto : CleaningSessionDto
{
    public List<Guid> ZoneIds { get; set; } = new();
    public List<string> ZoneNames { get; set; } = new();
    public List<CleaningErrorDto> Errors { get; set; } = new();
}

public class CleaningErrorDto
{
    public Guid Id { get; set; }
    public string ErrorCode { get; set; } = null!;
    public string Message { get; set; } = null!;
    public DateTime Timestamp { get; set; }
    public bool IsResolved { get; set; }
}

public class StartCleaningDto
{
    public CleaningMode Mode { get; set; }
    public List<Guid> ZoneIds { get; set; } = new();
}