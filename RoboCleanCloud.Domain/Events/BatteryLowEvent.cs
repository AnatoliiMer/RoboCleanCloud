using System;
using MediatR;

namespace RoboCleanCloud.Domain.Events;

public class BatteryLowEvent : INotification
{
    public Guid RobotId { get; }
    public int BatteryLevel { get; }
    public DateTime OccurredOn { get; }

    public BatteryLowEvent(Guid robotId, int batteryLevel)
    {
        RobotId = robotId;
        BatteryLevel = batteryLevel;
        OccurredOn = DateTime.UtcNow;
    }
}