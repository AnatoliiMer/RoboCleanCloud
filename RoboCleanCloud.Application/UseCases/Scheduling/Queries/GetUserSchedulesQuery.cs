using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RoboCleanCloud.Application.DTOs;
using RoboCleanCloud.Application.Interfaces.Repositories;

namespace RoboCleanCloud.Application.UseCases.Scheduling.Queries;

public record GetUserSchedulesQuery(Guid UserId) : IRequest<List<CleaningScheduleDto>>;

public class GetUserSchedulesQueryHandler : IRequestHandler<GetUserSchedulesQuery, List<CleaningScheduleDto>>
{
    private readonly ICleaningScheduleRepository _scheduleRepository;
    private readonly IRobotRepository _robotRepository;

    public GetUserSchedulesQueryHandler(
        ICleaningScheduleRepository scheduleRepository,
        IRobotRepository robotRepository)
    {
        _scheduleRepository = scheduleRepository;
        _robotRepository = robotRepository;
    }

    public async Task<List<CleaningScheduleDto>> Handle(GetUserSchedulesQuery request, CancellationToken cancellationToken)
    {
        var robots = await _robotRepository.GetByOwnerIdAsync(request.UserId, cancellationToken);
        var robotIds = robots.Select(r => r.Id).ToList();

        var allSchedules = new List<CleaningScheduleDto>();

        foreach (var robotId in robotIds)
        {
            var schedules = await _scheduleRepository.GetByRobotIdAsync(robotId, cancellationToken);
            allSchedules.AddRange(schedules.Select(s => new CleaningScheduleDto
            {
                Id = s.Id,
                RobotId = s.RobotId,
                RobotName = robots.First(r => r.Id == s.RobotId).FriendlyName,
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
            }));
        }

        return allSchedules;
    }
}