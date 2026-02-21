using System;
using MediatR;

namespace RoboCleanCloud.Domain.Events;

public class ScheduleDeactivatedEvent : INotification
{
    public Guid ScheduleId { get; }
    public Guid RobotId { get; }
    public DateTime OccurredOn { get; }

    public ScheduleDeactivatedEvent(Guid scheduleId, Guid robotId)
    {
        ScheduleId = scheduleId;
        RobotId = robotId;
        OccurredOn = DateTime.UtcNow;
    }
}