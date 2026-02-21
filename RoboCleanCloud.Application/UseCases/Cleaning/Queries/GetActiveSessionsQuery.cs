using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RoboCleanCloud.Application.DTOs;
using RoboCleanCloud.Application.Interfaces.Repositories;
using RoboCleanCloud.Domain.Enums;

namespace RoboCleanCloud.Application.UseCases.Cleaning.Queries;

public record GetActiveSessionsQuery(Guid? UserId = null) : IRequest<PagedResult<CleaningSessionDto>>;

public class GetActiveSessionsQueryHandler : IRequestHandler<GetActiveSessionsQuery, PagedResult<CleaningSessionDto>>
{
    private readonly ICleaningSessionRepository _sessionRepository;

    public GetActiveSessionsQueryHandler(ICleaningSessionRepository sessionRepository)
    {
        _sessionRepository = sessionRepository;
    }

    public async Task<PagedResult<CleaningSessionDto>> Handle(GetActiveSessionsQuery request, CancellationToken cancellationToken)
    {
        var sessions = await _sessionRepository.GetByStatusAsync(CleaningSessionStatus.InProgress, cancellationToken);

        var items = sessions.Select(s => new CleaningSessionDto
        {
            Id = s.Id,
            RobotId = s.RobotId,
            RobotName = "Unknown",
            Mode = s.Mode,
            Status = s.Status,
            StartedAt = s.StartedAt,
            FinishedAt = s.FinishedAt,
            AreaCleaned = s.AreaCleaned,
            EnergyConsumed = s.EnergyConsumed,
            Progress = 50,
            EstimatedRemainingMinutes = 30
        }).ToList();

        return new PagedResult<CleaningSessionDto>
        {
            Items = items,
            Page = 1,
            PageSize = items.Count,
            TotalCount = items.Count
        };
    }
}