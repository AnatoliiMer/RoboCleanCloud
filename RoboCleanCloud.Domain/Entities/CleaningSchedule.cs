using System;
using System.Collections.Generic;
using RoboCleanCloud.Domain.Enums;
using RoboCleanCloud.Domain.Primitives;
using RoboCleanCloud.Domain.Events;
using RoboCleanCloud.Domain.Exceptions;
using NCrontab;

namespace RoboCleanCloud.Domain.Entities;

public class CleaningSchedule : AggregateRoot
{
    private CleaningSchedule() { }

    public CleaningSchedule(
        Guid robotId,
        string cronExpression,
        CleaningMode mode,
        List<Guid>? zoneIds = null,
        string timeZone = "UTC")
    {
        Id = Guid.NewGuid();
        RobotId = robotId;
        CronExpression = cronExpression ?? throw new ArgumentNullException(nameof(cronExpression));
        Mode = mode;
        ZoneIds = zoneIds ?? new List<Guid>();
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        TimeZone = timeZone ?? throw new ArgumentNullException(nameof(timeZone));
        ValidateCronExpression();
    }

    public Guid RobotId { get; private set; }
    public Robot? Robot { get; private set; }
    public string CronExpression { get; private set; } = null!;
    public CleaningMode Mode { get; private set; }
    public List<Guid> ZoneIds { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastTriggeredAt { get; private set; }
    public string TimeZone { get; private set; } = null!;
    public int? QuietHoursStart { get; private set; }
    public int? QuietHoursEnd { get; private set; }

    public void Activate()
    {
        IsActive = true;
        AddDomainEvent(new ScheduleActivatedEvent(Id, RobotId));
    }

    public void Deactivate()
    {
        IsActive = false;
        AddDomainEvent(new ScheduleDeactivatedEvent(Id, RobotId));
    }

    public void UpdateCron(string newExpression)
    {
        CronExpression = newExpression ?? throw new ArgumentNullException(nameof(newExpression));
        ValidateCronExpression();
        AddDomainEvent(new ScheduleUpdatedEvent(Id, RobotId, newExpression));
    }

    private void ValidateCronExpression()
    {
        try
        {
            CrontabSchedule.Parse(CronExpression);
        }
        catch (Exception ex)
        {
            throw new DomainException($"Invalid cron expression: {ex.Message}");
        }
    }

    public DateTime? GetNextExecution()
    {
        if (!IsActive) return null;

        var schedule = CrontabSchedule.Parse(CronExpression);
        var now = DateTime.UtcNow;
        var next = schedule.GetNextOccurrence(now);

        return next;
    }

    public void MarkTriggered()
    {
        LastTriggeredAt = DateTime.UtcNow;
        AddDomainEvent(new ScheduleTriggeredEvent(Id, RobotId, DateTime.UtcNow));
    }

    public void SetQuietHours(int startHour, int endHour)
    {
        if (startHour < 0 || startHour > 23 || endHour < 0 || endHour > 23)
            throw new DomainException("Quiet hours must be between 0 and 23");

        QuietHoursStart = startHour;
        QuietHoursEnd = endHour;
        AddDomainEvent(new QuietHoursUpdatedEvent(Id, RobotId, startHour, endHour));
    }
}