using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RoboCleanCloud.Application.DTOs;
using RoboCleanCloud.Application.Exceptions;
using RoboCleanCloud.Application.Interfaces.Repositories;

namespace RoboCleanCloud.Application.UseCases.Scheduling.Queries;

public record GetScheduleQuery(Guid ScheduleId) : IRequest<CleaningScheduleDto>;

public class GetScheduleQueryHandler : IRequestHandler<GetScheduleQuery, CleaningScheduleDto>
{
    private readonly ICleaningScheduleRepository _scheduleRepository;

    public GetScheduleQueryHandler(ICleaningScheduleRepository scheduleRepository)
    {
        _scheduleRepository = scheduleRepository;
    }

    public async Task<CleaningScheduleDto> Handle(GetScheduleQuery request, CancellationToken cancellationToken)
    {
        var schedule = await _scheduleRepository.GetByIdAsync(request.ScheduleId, cancellationToken);
        if (schedule == null)
            throw new NotFoundException($"Schedule with ID {request.ScheduleId} not found");

        return new CleaningScheduleDto
        {
            Id = schedule.Id,
            RobotId = schedule.RobotId,
            RobotName = "Unknown",
            CronExpression = schedule.CronExpression,
            Mode = schedule.Mode,
            ZoneIds = schedule.ZoneIds,
            IsActive = schedule.IsActive,
            CreatedAt = schedule.CreatedAt,
            LastTriggeredAt = schedule.LastTriggeredAt,
            NextExecution = schedule.GetNextExecution(),
            TimeZone = schedule.TimeZone,
            QuietHoursStart = schedule.QuietHoursStart,
            QuietHoursEnd = schedule.QuietHoursEnd
        };
    }
}