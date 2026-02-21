using System;
using MediatR;

namespace RoboCleanCloud.Domain.Events;

public class RobotReturningToBaseEvent : INotification
{
    public Guid RobotId { get; }
    public DateTime OccurredOn { get; }

    public RobotReturningToBaseEvent(Guid robotId)
    {
        RobotId = robotId;
        OccurredOn = DateTime.UtcNow;
    }
}