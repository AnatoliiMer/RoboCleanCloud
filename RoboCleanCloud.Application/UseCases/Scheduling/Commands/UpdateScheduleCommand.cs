using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RoboCleanCloud.Application.Exceptions;
using RoboCleanCloud.Application.Interfaces.Repositories;
using RoboCleanCloud.Application.Interfaces.Services;
using RoboCleanCloud.Domain.Enums;

namespace RoboCleanCloud.Application.UseCases.Scheduling.Commands;

public record UpdateScheduleCommand(
    Guid ScheduleId,
    string? CronExpression = null,
    CleaningMode? Mode = null,
    List<Guid>? ZoneIds = null,
    int? QuietHoursStart = null,
    int? QuietHoursEnd = null) : IRequest;

public class UpdateScheduleCommandHandler : IRequestHandler<UpdateScheduleCommand>
{
    private readonly ICleaningScheduleRepository _scheduleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateScheduleCommandHandler(
        ICleaningScheduleRepository scheduleRepository,
        IUnitOfWork unitOfWork)
    {
        _scheduleRepository = scheduleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateScheduleCommand request, CancellationToken cancellationToken)
    {
        var schedule = await _scheduleRepository.GetByIdAsync(request.ScheduleId, cancellationToken);
        if (schedule == null)
            throw new NotFoundException($"Schedule with ID {request.ScheduleId} not found");

        if (!string.IsNullOrWhiteSpace(request.CronExpression))
        {
            schedule.UpdateCron(request.CronExpression);
        }

        if (request.Mode.HasValue)
        {
            // В реальном проекте здесь нужно обновить режим
        }

        if (request.QuietHoursStart.HasValue && request.QuietHoursEnd.HasValue)
        {
            schedule.SetQuietHours(request.QuietHoursStart.Value, request.QuietHoursEnd.Value);
        }

        _scheduleRepository.Update(schedule);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}