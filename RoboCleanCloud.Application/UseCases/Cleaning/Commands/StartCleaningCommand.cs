using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RoboCleanCloud.Application.Interfaces.Repositories;
using RoboCleanCloud.Application.Interfaces.Services;
using RoboCleanCloud.Domain.Entities;
using RoboCleanCloud.Domain.Enums;
using RoboCleanCloud.Domain.Exceptions;

namespace RoboCleanCloud.Application.UseCases.Cleaning.Commands;

public record StartCleaningCommand(
    Guid RobotId,
    CleaningMode Mode,
    List<Guid> ZoneIds,
    bool IsScheduled = false,
    Guid? ScheduleId = null) : IRequest<CleaningSessionResponse>;

public record CleaningSessionResponse(
    Guid SessionId,
    Guid RobotId,
    CleaningMode Mode,
    CleaningSessionStatus Status,
    DateTime StartedAt);

public class StartCleaningCommandHandler : IRequestHandler<StartCleaningCommand, CleaningSessionResponse>
{
    private readonly IRobotRepository _robotRepository;
    private readonly ICleaningSessionRepository _sessionRepository;
    private readonly IRobotCommandGateway _robotCommandGateway;
    private readonly IUnitOfWork _unitOfWork;

    public StartCleaningCommandHandler(
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

    public async Task<CleaningSessionResponse> Handle(
        StartCleaningCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Получаем робота
        var robot = await _robotRepository.GetByIdAsync(request.RobotId, cancellationToken);
        if (robot == null)
            throw new NotFoundException($"Robot with ID {request.RobotId} not found");

        // 2. Проверяем возможность уборки
        if (!robot.CanStartCleaning())
            throw new DomainException("Robot is not ready for cleaning");

        // 3. Создаем сессию уборки
        var session = new CleaningSession(
            request.RobotId,
            request.Mode,
            request.ZoneIds,
            request.ScheduleId);

        await _sessionRepository.AddAsync(session, cancellationToken);

        // 4. Обновляем статус робота
        robot.StartCleaning();
        _robotRepository.Update(robot);  // ТЕПЕРЬ РАБОТАЕТ!

        // 5. Сохраняем изменения
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 6. Отправляем команду роботу
        await _robotCommandGateway.SendCleaningCommandAsync(
            request.RobotId,
            session.Id,
            request.Mode,
            request.ZoneIds,
            cancellationToken);

        return new CleaningSessionResponse(
            session.Id,
            session.RobotId,
            session.Mode,
            session.Status,
            session.StartedAt);
    }
}