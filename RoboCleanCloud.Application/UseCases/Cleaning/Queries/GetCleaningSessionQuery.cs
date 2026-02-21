using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RoboCleanCloud.Application.DTOs;
using RoboCleanCloud.Application.Exceptions;
using RoboCleanCloud.Application.Interfaces.Repositories;

namespace RoboCleanCloud.Application.UseCases.Cleaning.Queries;

public record GetCleaningSessionQuery(Guid SessionId) : IRequest<CleaningSessionDetailsDto>;

public class GetCleaningSessionQueryHandler : IRequestHandler<GetCleaningSessionQuery, CleaningSessionDetailsDto>
{
    private readonly ICleaningSessionRepository _sessionRepository;

    public GetCleaningSessionQueryHandler(ICleaningSessionRepository sessionRepository)
    {
        _sessionRepository = sessionRepository;
    }

    public async Task<CleaningSessionDetailsDto> Handle(GetCleaningSessionQuery request, CancellationToken cancellationToken)
    {
        var session = await _sessionRepository.GetByIdAsync(request.SessionId, cancellationToken);
        if (session == null)
            throw new NotFoundException($"Cleaning session with ID {request.SessionId} not found");

        return new CleaningSessionDetailsDto
        {
            Id = session.Id,
            RobotId = session.RobotId,
            RobotName = "Unknown",
            Mode = session.Mode,
            Status = session.Status,
            StartedAt = session.StartedAt,
            FinishedAt = session.FinishedAt,
            AreaCleaned = session.AreaCleaned,
            EnergyConsumed = session.EnergyConsumed,
            Progress = session.Status == CleaningSessionStatus.InProgress ? 50 : 100,
            EstimatedRemainingMinutes = null,
            ZoneIds = session.ZoneIds,
            ZoneNames = session.ZoneIds.Select(id => $"Zone-{id}").ToList(),
            Errors = session.Errors.Select(e => new CleaningErrorDto
            {
                Id = e.Id,
                ErrorCode = e.ErrorCode,
                Message = e.Message,
                Timestamp = e.Timestamp,
                IsResolved = e.IsResolved
            }).ToList()
        };
    }
}