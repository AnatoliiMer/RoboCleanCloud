using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RoboCleanCloud.Application.DTOs;
using RoboCleanCloud.Application.Interfaces.Repositories;

namespace RoboCleanCloud.Application.UseCases.Cleaning.Queries;

public record GetRobotActiveSessionQuery(Guid RobotId) : IRequest<CleaningSessionDto?>;

public class GetRobotActiveSessionQueryHandler : IRequestHandler<GetRobotActiveSessionQuery, CleaningSessionDto?>
{
    private readonly ICleaningSessionRepository _sessionRepository;

    public GetRobotActiveSessionQueryHandler(ICleaningSessionRepository sessionRepository)
    {
        _sessionRepository = sessionRepository;
    }

    public async Task<CleaningSessionDto?> Handle(GetRobotActiveSessionQuery request, CancellationToken cancellationToken)
    {
        var session = await _sessionRepository.GetActiveSessionAsync(request.RobotId, cancellationToken);

        if (session == null)
            return null;

        return new CleaningSessionDto
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
            Progress = 50,
            EstimatedRemainingMinutes = 30
        };
    }
}