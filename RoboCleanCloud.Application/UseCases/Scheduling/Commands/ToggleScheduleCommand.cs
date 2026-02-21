using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RoboCleanCloud.Application.Exceptions;
using RoboCleanCloud.Application.Interfaces.Repositories;
using RoboCleanCloud.Application.Interfaces.Services;

namespace RoboCleanCloud.Application.UseCases.Scheduling.Commands;

public record ToggleScheduleCommand(Guid ScheduleId, bool Activate) : IRequest;

public class ToggleScheduleCommandHandler : IRequestHandler<ToggleScheduleCommand>
{
    private readonly ICleaningScheduleRepository _scheduleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ToggleScheduleCommandHandler(
        ICleaningScheduleRepository scheduleRepository,
        IUnitOfWork unitOfWork)
    {
        _scheduleRepository = scheduleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ToggleScheduleCommand request, CancellationToken cancellationToken)
    {
        var schedule = await _scheduleRepository.GetByIdAsync(request.ScheduleId, cancellationToken);
        if (schedule == null)
            throw new NotFoundException($"Schedule with ID {request.ScheduleId} not found");

        if (request.Activate)
        {
            schedule.Activate();
        }
        else
        {
            schedule.Deactivate();
        }

        _scheduleRepository.Update(schedule);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}