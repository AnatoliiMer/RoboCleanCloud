using System;
using MediatR;

namespace RoboCleanCloud.Domain.Events;

public class ScheduleTriggeredEvent : INotification
{
    public Guid ScheduleId { get; }
    public Guid RobotId { get; }
    public DateTime TriggeredAt { get; }

    public ScheduleTriggeredEvent(Guid scheduleId, Guid robotId, DateTime triggeredAt)
    {
        ScheduleId = scheduleId;
        RobotId = robotId;
        TriggeredAt = triggeredAt;
    }
}