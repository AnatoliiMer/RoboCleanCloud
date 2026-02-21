using System;
using MediatR;
using RoboCleanCloud.Domain.Enums;

namespace RoboCleanCloud.Domain.Events;

public class RobotStatusChangedEvent : INotification
{
    public Guid RobotId { get; }
    public ConnectionStatus PreviousStatus { get; }
    public ConnectionStatus NewStatus { get; }
    public DateTime OccurredOn { get; }

    public RobotStatusChangedEvent(Guid robotId, ConnectionStatus previousStatus, ConnectionStatus newStatus)
    {
        RobotId = robotId;
        PreviousStatus = previousStatus;
        NewStatus = newStatus;
        OccurredOn = DateTime.UtcNow;
    }
}