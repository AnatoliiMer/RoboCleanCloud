using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RoboCleanCloud.Application.Exceptions;
using RoboCleanCloud.Application.Interfaces.Repositories;
using RoboCleanCloud.Application.Interfaces.Services;

namespace RoboCleanCloud.Application.UseCases.Scheduling.Commands;

public record DeleteScheduleCommand(Guid ScheduleId) : IRequest;

public class DeleteScheduleCommandHandler : IRequestHandler<DeleteScheduleCommand>
{
    private readonly ICleaningScheduleRepository _scheduleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteScheduleCommandHandler(
        ICleaningScheduleRepository scheduleRepository,
        IUnitOfWork unitOfWork)
    {
        _scheduleRepository = scheduleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteScheduleCommand request, CancellationToken cancellationToken)
    {
        var schedule = await _scheduleRepository.GetByIdAsync(request.ScheduleId, cancellationToken);
        if (schedule == null)
            throw new NotFoundException($"Schedule with ID {request.ScheduleId} not found");

        _scheduleRepository.Delete(schedule);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}