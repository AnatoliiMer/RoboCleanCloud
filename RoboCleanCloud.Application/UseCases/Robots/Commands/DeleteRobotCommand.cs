using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RoboCleanCloud.Application.Exceptions;
using RoboCleanCloud.Application.Interfaces.Repositories;

namespace RoboCleanCloud.Application.UseCases.Robots.Commands;

public record DeleteRobotCommand(Guid RobotId) : IRequest;

public class DeleteRobotCommandHandler : IRequestHandler<DeleteRobotCommand>
{
    private readonly IRobotRepository _robotRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteRobotCommandHandler(
        IRobotRepository robotRepository,
        IUnitOfWork unitOfWork)
    {
        _robotRepository = robotRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteRobotCommand request, CancellationToken cancellationToken)
    {
        var robot = await _robotRepository.GetByIdAsync(request.RobotId, cancellationToken);
        if (robot == null)
            throw new NotFoundException($"Robot with ID {request.RobotId} not found");

        _robotRepository.Delete(robot);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}