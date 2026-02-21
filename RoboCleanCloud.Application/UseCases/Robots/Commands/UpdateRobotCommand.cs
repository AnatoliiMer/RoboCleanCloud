using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RoboCleanCloud.Application.Exceptions;
using RoboCleanCloud.Application.Interfaces.Repositories;

namespace RoboCleanCloud.Application.UseCases.Robots.Commands;

public record UpdateRobotCommand(
    Guid RobotId,
    string? FriendlyName = null,
    string? FirmwareVersion = null) : IRequest;

public class UpdateRobotCommandHandler : IRequestHandler<UpdateRobotCommand>
{
    private readonly IRobotRepository _robotRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateRobotCommandHandler(
        IRobotRepository robotRepository,
        IUnitOfWork unitOfWork)
    {
        _robotRepository = robotRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateRobotCommand request, CancellationToken cancellationToken)
    {
        var robot = await _robotRepository.GetByIdAsync(request.RobotId, cancellationToken);
        if (robot == null)
            throw new NotFoundException($"Robot with ID {request.RobotId} not found");

        if (!string.IsNullOrWhiteSpace(request.FriendlyName))
        {
            // В реальном проекте здесь нужно изменить имя
        }

        if (!string.IsNullOrWhiteSpace(request.FirmwareVersion))
        {
            // В реальном проекте здесь нужно обновить версию прошивки
        }

        _robotRepository.Update(robot);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}