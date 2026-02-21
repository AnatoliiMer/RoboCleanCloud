using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RoboCleanCloud.Application.DTOs;
using RoboCleanCloud.Application.Exceptions;
using RoboCleanCloud.Application.Interfaces.Repositories;
using RoboCleanCloud.Application.Interfaces.Services;
using RoboCleanCloud.Domain.Entities;
using RoboCleanCloud.Domain.Enums;
using RoboCleanCloud.Domain.Exceptions;

namespace RoboCleanCloud.Application.UseCases.Scheduling.Commands;

public record CreateScheduleCommand(
    Guid RobotId,
    string CronExpression,
    CleaningMode Mode,
    List<Guid> ZoneIds,
    string TimeZone = "UTC",
    int? QuietHoursStart = null,
    int? QuietHoursEnd = null) : IRequest<ScheduleResponse>;

public record ScheduleResponse(
    Guid ScheduleId,
    Guid RobotId,
    string CronExpression,
    CleaningMode Mode,
    bool IsActive,
    DateTime? NextExecution);

public class CreateScheduleCommandHandler : IRequestHandler<CreateScheduleCommand, ScheduleResponse>
{
    private readonly ICleaningScheduleRepository _scheduleRepository;
    private readonly IRobotRepository _robotRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateScheduleCommandHandler(
        ICleaningScheduleRepository scheduleRepository,
        IRobotRepository robotRepository,
        IUnitOfWork unitOfWork)
    {
        _scheduleRepository = scheduleRepository;
        _robotRepository = robotRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ScheduleResponse> Handle(CreateScheduleCommand request, CancellationToken cancellationToken)
    {
        // Проверяем существование робота
        var robot = await _robotRepository.GetByIdAsync(request.RobotId, cancellationToken);
        if (robot == null)
            throw new NotFoundException($"Robot with ID {request.RobotId} not found");

        // Создаем расписание
        var schedule = new CleaningSchedule(
            request.RobotId,
            request.CronExpression,
            request.Mode,
            request.ZoneIds,
            request.TimeZone);

        if (request.QuietHoursStart.HasValue && request.QuietHoursEnd.HasValue)
        {
            schedule.SetQuietHours(request.QuietHoursStart.Value, request.QuietHoursEnd.Value);
        }

        await _scheduleRepository.AddAsync(schedule, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ScheduleResponse(
            schedule.Id,
            schedule.RobotId,
            schedule.CronExpression,
            schedule.Mode,
            schedule.IsActive,
            schedule.GetNextExecution());
    }
}