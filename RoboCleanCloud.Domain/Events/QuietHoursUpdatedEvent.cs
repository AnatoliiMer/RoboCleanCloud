using System;
using MediatR;

namespace RoboCleanCloud.Domain.Events;

public class QuietHoursUpdatedEvent : INotification
{
    public Guid ScheduleId { get; }
    public Guid RobotId { get; }
    public int StartHour { get; }
    public int EndHour { get; }
    public DateTime OccurredOn { get; }

    public QuietHoursUpdatedEvent(Guid scheduleId, Guid robotId, int startHour, int endHour)
    {
        ScheduleId = scheduleId;
        RobotId = robotId;
        StartHour = startHour;
        EndHour = endHour;
        OccurredOn = DateTime.UtcNow;
    }
}