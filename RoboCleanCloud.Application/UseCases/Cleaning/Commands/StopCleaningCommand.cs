using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RoboCleanCloud.Application.Interfaces.Repositories;
using RoboCleanCloud.Application.Interfaces.Services;
using RoboCleanCloud.Domain.Exceptions;

namespace RoboCleanCloud.Application.UseCases.Cleaning.Commands;

public record StopCleaningCommand(Guid RobotId) : IRequest;

public class StopCleaningCommandHandler : IRequestHandler<StopCleaningCommand>
{
    private readonly IRobotRepository _robotRepository;
    private readonly ICleaningSessionRepository _sessionRepository;
    private readonly IRobotCommandGateway _robotCommandGateway;
    private readonly IUnitOfWork _unitOfWork;

    public StopCleaningCommandHandler(
        IRobotRepository robotRepository,
        ICleaningSessionRepository sessionRepository,
        IRobotCommandGateway robotCommandGateway,
        IUnitOfWork unitOfWork)
    {
        _robotRepository = robotRepository;
        _sessionRepository = sessionRepository;
        _robotCommandGateway = robotCommandGateway;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(StopCleaningCommand request, CancellationToken cancellationToken)
    {
        var robot = await _robotRepository.GetByIdAsync(request.RobotId, cancellationToken);
        if (robot == null)
            throw new NotFoundException($"Robot with ID {request.RobotId} not found");

        var activeSession = await _sessionRepository.GetActiveSessionAsync(request.RobotId, cancellationToken);
        if (activeSession != null)
        {
            activeSession.Cancel();
            _sessionRepository.Update(activeSession);
        }

        robot.ReturnToBase();
        _robotRepository.Update(robot);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _robotCommandGateway.SendStopCommandAsync(request.RobotId, cancellationToken);
    }
}