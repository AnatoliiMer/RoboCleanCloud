using System;
using System.Collections.Generic;
using RoboCleanCloud.Domain.Enums;
using RoboCleanCloud.Domain.Primitives;
using RoboCleanCloud.Domain.Exceptions;

namespace RoboCleanCloud.Domain.Entities;

public class CleaningSession : Entity
{
    private CleaningSession() { }

    public CleaningSession(
        Guid robotId,
        CleaningMode mode,
        List<Guid>? zoneIds = null,
        Guid? scheduleId = null)
    {
        Id = Guid.NewGuid();
        RobotId = robotId;
        Mode = mode;
        ZoneIds = zoneIds ?? new List<Guid>();
        ScheduleId = scheduleId;
        Status = CleaningSessionStatus.Planned;
        StartedAt = DateTime.UtcNow;
        Errors = new List<CleaningError>();
    }

    public Guid RobotId { get; private set; }
    public Robot? Robot { get; private set; }
    public CleaningMode Mode { get; private set; }
    public List<Guid> ZoneIds { get; private set; } = null!;
    public Guid? ScheduleId { get; private set; }
    public CleaningSchedule? Schedule { get; private set; }
    public CleaningSessionStatus Status { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? FinishedAt { get; private set; }
    public double? AreaCleaned { get; private set; }
    public double? EnergyConsumed { get; private set; }
    public List<CleaningError> Errors { get; private set; } = null!;

    public void Start()
    {
        if (Status != CleaningSessionStatus.Planned)
            throw new DomainException("Session already started");

        Status = CleaningSessionStatus.InProgress;
    }

    public void Complete(double area, double energy)
    {
        Status = CleaningSessionStatus.Completed;
        FinishedAt = DateTime.UtcNow;
        AreaCleaned = area;
        EnergyConsumed = energy;
    }

    public void Fail(string errorCode, string? errorMessage = null)
    {
        Status = CleaningSessionStatus.Failed;
        FinishedAt = DateTime.UtcNow;
        Errors.Add(new CleaningError(Id, errorCode, errorMessage));
    }

    public void AddError(string errorCode, string? errorMessage = null)
    {
        Errors.Add(new CleaningError(Id, errorCode, errorMessage));
    }

    public void Pause()
    {
        if (Status != CleaningSessionStatus.InProgress)
            throw new DomainException("Cannot pause session that is not in progress");

        Status = CleaningSessionStatus.Paused;
    }

    public void Resume()
    {
        if (Status != CleaningSessionStatus.Paused)
            throw new DomainException("Cannot resume session that is not paused");

        Status = CleaningSessionStatus.InProgress;
    }

    public void Cancel()
    {
        if (Status == CleaningSessionStatus.Completed || Status == CleaningSessionStatus.Failed)
            throw new DomainException("Cannot cancel completed or failed session");

        Status = CleaningSessionStatus.Cancelled;
        FinishedAt = DateTime.UtcNow;
    }
}