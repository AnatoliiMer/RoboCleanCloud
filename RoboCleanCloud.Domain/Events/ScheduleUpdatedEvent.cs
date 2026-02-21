using System;
using MediatR;

namespace RoboCleanCloud.Domain.Events;

public class ScheduleUpdatedEvent : INotification
{
    public Guid ScheduleId { get; }
    public Guid RobotId { get; }
    public string NewCronExpression { get; }
    public DateTime OccurredOn { get; }

    public ScheduleUpdatedEvent(Guid scheduleId, Guid robotId, string newCronExpression)
    {
        ScheduleId = scheduleId;
        RobotId = robotId;
        NewCronExpression = newCronExpression;
        OccurredOn = DateTime.UtcNow;
    }
}