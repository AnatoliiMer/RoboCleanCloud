using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RoboCleanCloud.Application.DTOs;
using RoboCleanCloud.Application.Interfaces.Repositories;

namespace RoboCleanCloud.Application.UseCases.Scheduling.Queries;

public record GetRobotSchedulesQuery(Guid RobotId) : IRequest<List<CleaningScheduleDto>>;

public class GetRobotSchedulesQueryHandler : IRequestHandler<GetRobotSchedulesQuery, List<CleaningScheduleDto>>
{
    private readonly ICleaningScheduleRepository _scheduleRepository;

    public GetRobotSchedulesQueryHandler(ICleaningScheduleRepository scheduleRepository)
    {
        _scheduleRepository = scheduleRepository;
    }

    public async Task<List<CleaningScheduleDto>> Handle(GetRobotSchedulesQuery request, CancellationToken cancellationToken)
    {
        var schedules = await _scheduleRepository.GetByRobotIdAsync(request.RobotId, cancellationToken);

        return schedules.Select(s => new CleaningScheduleDto
        {
            Id = s.Id,
            RobotId = s.RobotId,
            RobotName = "Unknown",
            CronExpression = s.CronExpression,
            Mode = s.Mode,
            ZoneIds = s.ZoneIds,
            IsActive = s.IsActive,
            CreatedAt = s.CreatedAt,
            LastTriggeredAt = s.LastTriggeredAt,
            NextExecution = s.GetNextExecution(),
            TimeZone = s.TimeZone,
            QuietHoursStart = s.QuietHoursStart,
            QuietHoursEnd = s.QuietHoursEnd
        }).ToList();
    }
}