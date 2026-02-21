using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RoboCleanCloud.Application.Exceptions;
using RoboCleanCloud.Application.Interfaces.Repositories;
using RoboCleanCloud.Application.Interfaces.Services;

namespace RoboCleanCloud.Application.UseCases.Cleaning.Commands;

public record ReturnToBaseCommand(Guid RobotId) : IRequest;

public class ReturnToBaseCommandHandler : IRequestHandler<ReturnToBaseCommand>
{
    private readonly IRobotRepository _robotRepository;
    private readonly IRobotCommandGateway _robotCommandGateway;
    private readonly IUnitOfWork _unitOfWork;

    public ReturnToBaseCommandHandler(
        IRobotRepository robotRepository,
        IRobotCommandGateway robotCommandGateway,
        IUnitOfWork unitOfWork)
    {
        _robotRepository = robotRepository;
        _robotCommandGateway = robotCommandGateway;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ReturnToBaseCommand request, CancellationToken cancellationToken)
    {
        var robot = await _robotRepository.GetByIdAsync(request.RobotId, cancellationToken);
        if (robot == null)
            throw new NotFoundException($"Robot with ID {request.RobotId} not found");

        robot.ReturnToBase();
        _robotRepository.Update(robot);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _robotCommandGateway.SendReturnToBaseCommandAsync(request.RobotId, cancellationToken);
    }
}