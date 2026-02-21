using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RoboCleanCloud.Application.DTOs;
using RoboCleanCloud.Application.Exceptions;
using RoboCleanCloud.Application.Interfaces.Repositories;

namespace RoboCleanCloud.Application.UseCases.Robots.Queries;

public record GetRobotQuery(Guid RobotId) : IRequest<RobotDetailsDto>;

public class GetRobotQueryHandler : IRequestHandler<GetRobotQuery, RobotDetailsDto>
{
    private readonly IRobotRepository _robotRepository;
    private readonly ICleaningSessionRepository _sessionRepository;

    public GetRobotQueryHandler(
        IRobotRepository robotRepository,
        ICleaningSessionRepository sessionRepository)
    {
        _robotRepository = robotRepository;
        _sessionRepository = sessionRepository;
    }

    public async Task<RobotDetailsDto> Handle(GetRobotQuery request, CancellationToken cancellationToken)
    {
        var robot = await _robotRepository.GetByIdAsync(request.RobotId, cancellationToken);
        if (robot == null)
            throw new NotFoundException($"Robot with ID {request.RobotId} not found");

        var recentSessions = await _sessionRepository.GetByRobotIdAsync(request.RobotId, cancellationToken);

        return new RobotDetailsDto
        {
            Id = robot.Id,
            SerialNumber = robot.SerialNumber,
            Model = robot.Model,
            FriendlyName = robot.FriendlyName,
            OwnerId = robot.OwnerId,
            ConnectionStatus = robot.ConnectionStatus,
            BatteryLevel = robot.BatteryLevel,
            FirmwareVersion = robot.FirmwareVersion,
            RegisteredAt = robot.RegisteredAt,
            LastSeenAt = robot.LastSeenAt,
            DustbinLevel = robot.DustbinLevel,
            StatusHistory = robot.StatusHistory.Select(h => new RobotStatusHistoryDto
            {
                Id = h.Id,
                PreviousStatus = h.PreviousStatus,
                NewStatus = h.NewStatus,
                Reason = h.Reason,
                Timestamp = h.Timestamp
            }).ToList(),
            MaintenanceItems = robot.MaintenanceItems.Select(m => new MaintenanceItemDto
            {
                Id = m.Id,
                Type = m.Type,
                CurrentHealth = m.CurrentHealth,
                LastReplacedAt = m.LastReplacedAt,
                EstimatedDaysLeft = m.EstimatedDaysLeft
            }).ToList(),
            RecentSessions = recentSessions.Take(10).Select(s => new CleaningSessionDto
            {
                Id = s.Id,
                RobotId = s.RobotId,
                RobotName = robot.FriendlyName,
                Mode = s.Mode,
                Status = s.Status,
                StartedAt = s.StartedAt,
                FinishedAt = s.FinishedAt,
                AreaCleaned = s.AreaCleaned,
                EnergyConsumed = s.EnergyConsumed,
                Progress = 100,
                EstimatedRemainingMinutes = null
            }).ToList()
        };
    }
}