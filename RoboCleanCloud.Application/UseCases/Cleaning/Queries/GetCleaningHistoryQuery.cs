using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RoboCleanCloud.Application.DTOs;
using RoboCleanCloud.Application.Interfaces.Repositories;

namespace RoboCleanCloud.Application.UseCases.Cleaning.Queries;

public record GetCleaningHistoryQuery(
    Guid? RobotId,
    DateTime? From,
    DateTime? To,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<CleaningSessionDto>>;

public class GetCleaningHistoryQueryHandler : IRequestHandler<GetCleaningHistoryQuery, PagedResult<CleaningSessionDto>>
{
    private readonly ICleaningSessionRepository _sessionRepository;

    public GetCleaningHistoryQueryHandler(ICleaningSessionRepository sessionRepository)
    {
        _sessionRepository = sessionRepository;
    }

    public async Task<PagedResult<CleaningSessionDto>> Handle(GetCleaningHistoryQuery request, CancellationToken cancellationToken)
    {
        // В реальном проекте здесь будет сложная логика с фильтрацией и пагинацией
        var sessions = await _sessionRepository.GetAllAsync(cancellationToken);

        var items = sessions.Select(s => new CleaningSessionDto
        {
            Id = s.Id,
            RobotId = s.RobotId,
            RobotName = "Unknown", // В реальном проекте нужно получать имя робота
            Mode = s.Mode,
            Status = s.Status,
            StartedAt = s.StartedAt,
            FinishedAt = s.FinishedAt,
            AreaCleaned = s.AreaCleaned,
            EnergyConsumed = s.EnergyConsumed,
            Progress = s.Status == CleaningSessionStatus.InProgress ? 50 : 100,
            EstimatedRemainingMinutes = null
        }).ToList();

        return new PagedResult<CleaningSessionDto>
        {
            Items = items,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = items.Count
        };
    }
}