using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoboCleanCloud.Application.UseCases.Cleaning.Queries;
using RoboCleanCloud.Application.UseCases.Robots.Queries;
using RoboCleanCloud.Application.UseCases.Scheduling.Queries;
using RoboCleanCloud.Api.Bff.Models;
using RoboCleanCloud.Application.DTOs;
using RoboCleanCloud.Api.Extensions;
using RoboCleanCloud.Domain.Enums;

namespace RoboCleanCloud.Api.Bff;

/// <summary>
/// BFF для мобильных клиентов - оптимизированный и агрегированный API
/// </summary>
[ApiController]
[Route("bff/mobile/v1")]
[Authorize]  //временно отключаем//
public class MobileBffController : ControllerBase
{
    private readonly IMediator _mediator;

    public MobileBffController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Полный дашборд для мобильного приложения
    /// Агрегирует: статусы роботов, текущие уборки, расписания, уведомления
    /// </summary>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(MobileDashboardResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<MobileDashboardResponse>> GetDashboard(
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized("User not authenticated");
        }

        // Параллельные запросы для оптимизации производительности
        var robotsTask = _mediator.Send(new GetUserRobotsQuery(userId, 1, 100), cancellationToken);
        var schedulesTask = _mediator.Send(new GetUserSchedulesQuery(userId), cancellationToken);
        var activeSessionsTask = _mediator.Send(new GetActiveSessionsQuery(userId), cancellationToken);

        await Task.WhenAll(robotsTask, schedulesTask, activeSessionsTask);

        var robots = await robotsTask;
        var schedules = await schedulesTask;
        var activeSessions = await activeSessionsTask;

        var dashboard = new MobileDashboardResponse
        {
            Robots = robots.Items.Select(r => new MobileRobotSummary
            {
                RobotId = r.Id,
                Name = r.FriendlyName,
                ConnectionStatus = r.ConnectionStatus.ToString(),
                BatteryLevel = r.BatteryLevel,
                DustbinLevel = r.DustbinLevel,
                IsCleaning = r.ConnectionStatus == ConnectionStatus.Busy,
                CurrentSession = activeSessions.Items
                    .FirstOrDefault(s => s.RobotId == r.Id)
            }).ToList(),

            Schedules = schedules.Select(s => new MobileScheduleSummary
            {
                ScheduleId = s.Id,
                RobotName = s.RobotName,
                Time = s.NextExecution?.ToString("HH:mm"),
                Days = GetDaysFromCron(s.CronExpression),
                IsActive = s.IsActive
            }).ToList(),

            Notifications = await GetUserNotifications(userId, 5, cancellationToken),

            Summary = new DashboardSummary
            {
                TotalRobots = robots.TotalCount,
                ActiveCleanings = activeSessions.TotalCount,
                TotalSchedules = schedules.Count,
                PendingMaintenance = await GetPendingMaintenanceCount(userId, cancellationToken)
            }
        };

        return Ok(dashboard);
    }

    /// <summary>
    /// Краткий статус для виджетов и push-уведомлений
    /// </summary>
    [HttpGet("robots/{robotId:guid}/status-summary")]
    [ProducesResponseType(typeof(RobotStatusSummary), StatusCodes.Status200OK)]
    public async Task<ActionResult<RobotStatusSummary>> GetRobotStatusSummary(
        Guid robotId,
        CancellationToken cancellationToken)
    {
        var robot = await _mediator.Send(new GetRobotQuery(robotId), cancellationToken);
        var activeSession = await _mediator.Send(
            new GetRobotActiveSessionQuery(robotId),
            cancellationToken);

        return Ok(new RobotStatusSummary
        {
            RobotId = robot.Id,
            Status = robot.ConnectionStatus.ToString(),
            BatteryLevel = robot.BatteryLevel,
            IsCleaning = activeSession != null,
            CleaningProgress = activeSession?.Progress ?? 0,
            EstimatedTimeRemaining = activeSession?.EstimatedRemainingMinutes,
            LastUpdateTime = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Получение последних уведомлений пользователя
    /// </summary>
    [HttpGet("notifications")]
    [ProducesResponseType(typeof(List<NotificationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<NotificationDto>>> GetNotifications(
        [FromQuery] int count = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        if (userId == Guid.Empty)
        {
            return Unauthorized("User not authenticated");
        }

        var notifications = await GetUserNotifications(userId, count, cancellationToken);
        return Ok(notifications);
    }

    /// <summary>
    /// Отметить уведомление как прочитанное
    /// </summary>
    [HttpPost("notifications/{notificationId:guid}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarkNotificationAsRead(
        Guid notificationId,
        CancellationToken cancellationToken)
    {
        // В реальном проекте здесь будет логика отметки уведомления как прочитанного
        return NoContent();
    }

    /// <summary>
    /// Быстрые действия с роботом
    /// </summary>
    [HttpPost("robots/{robotId:guid}/quick-action")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> QuickAction(
        Guid robotId,
        [FromBody] QuickActionRequest request,
        CancellationToken cancellationToken)
    {
        return request.Action switch
        {
            "start" => Accepted(), // В реальном проекте здесь будет запуск уборки
            "stop" => Accepted(),
            "pause" => Accepted(),
            "resume" => Accepted(),
            "home" => Accepted(),
            _ => BadRequest($"Unknown action: {request.Action}")
        };
    }

    #region Private Methods

    /// <summary>
    /// Получение уведомлений пользователя
    /// </summary>
    private async Task<List<NotificationDto>> GetUserNotifications(Guid userId, int count, CancellationToken cancellationToken)
    {
        // В реальном проекте здесь будет запрос к базе данных
        // Пока возвращаем тестовые данные для разработки
        await Task.Delay(10, cancellationToken); // Имитация асинхронной операции

        return new List<NotificationDto>
        {
            new NotificationDto
            {
                Id = Guid.NewGuid(),
                Type = "info",
                Title = "Уборка завершена",
                Message = "Робот закончил уборку в гостиной",
                Timestamp = DateTime.UtcNow.AddHours(-1),
                IsRead = false
            },
            new NotificationDto
            {
                Id = Guid.NewGuid(),
                Type = "warning",
                Title = "Низкий заряд батареи",
                Message = "Робот вернулся на базу для зарядки",
                Timestamp = DateTime.UtcNow.AddHours(-2),
                IsRead = true
            },
            new NotificationDto
            {
                Id = Guid.NewGuid(),
                Type = "maintenance",
                Title = "Требуется обслуживание",
                Message = "Фильтр нуждается в замене",
                Timestamp = DateTime.UtcNow.AddDays(-1),
                IsRead = false
            }
        }.Take(count).ToList();
    }

    /// <summary>
    /// Получение количества предметов, требующих обслуживания
    /// </summary>
    private async Task<int> GetPendingMaintenanceCount(Guid userId, CancellationToken cancellationToken)
    {
        // В реальном проекте здесь будет запрос к базе данных
        // Пока возвращаем тестовые данные для разработки
        await Task.Delay(5, cancellationToken); // Имитация асинхронной операции
        return 2;
    }

    /// <summary>
    /// Получение дней недели из cron выражения
    /// </summary>
    private string? GetDaysFromCron(string cronExpression)
    {
        if (string.IsNullOrEmpty(cronExpression))
            return null;

        // Простейшее преобразование cron в дни недели
        // В реальном проекте здесь должна быть более сложная логика с библиотекой NCrontab
        if (cronExpression.Contains("1-5")) return "Будни";
        if (cronExpression.Contains("0,6") || cronExpression.Contains("6,0")) return "Выходные";
        if (cronExpression.Contains("*")) return "Ежедневно";

        var days = new List<string>();
        if (cronExpression.Contains("1")) days.Add("Пн");
        if (cronExpression.Contains("2")) days.Add("Вт");
        if (cronExpression.Contains("3")) days.Add("Ср");
        if (cronExpression.Contains("4")) days.Add("Чт");
        if (cronExpression.Contains("5")) days.Add("Пт");
        if (cronExpression.Contains("6")) days.Add("Сб");
        if (cronExpression.Contains("0")) days.Add("Вс");

        return days.Count > 0 ? string.Join(", ", days) : null;
    }

    #endregion
}

/// <summary>
/// Модель запроса для быстрого действия
/// </summary>
public class QuickActionRequest
{
    public string Action { get; set; } = string.Empty;
    public Dictionary<string, object>? Parameters { get; set; }
}

// DTOs для мобильного BFF
public class MobileDashboardResponse
{
    public List<MobileRobotSummary> Robots { get; set; } = new();
    public List<MobileScheduleSummary> Schedules { get; set; } = new();
    public List<NotificationDto> Notifications { get; set; } = new();
    public DashboardSummary Summary { get; set; } = new();
}

public class MobileRobotSummary
{
    public Guid RobotId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ConnectionStatus { get; set; } = string.Empty;
    public int BatteryLevel { get; set; }
    public int DustbinLevel { get; set; }
    public bool IsCleaning { get; set; }
    public CleaningSessionDto? CurrentSession { get; set; }
}

public class MobileScheduleSummary
{
    public Guid ScheduleId { get; set; }
    public string RobotName { get; set; } = string.Empty;
    public string? Time { get; set; }
    public string? Days { get; set; }
    public bool IsActive { get; set; }
}

public class NotificationDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public bool IsRead { get; set; }
}

public class DashboardSummary
{
    public int TotalRobots { get; set; }
    public int ActiveCleanings { get; set; }
    public int TotalSchedules { get; set; }
    public int PendingMaintenance { get; set; }
}

public class RobotStatusSummary
{
    public Guid RobotId { get; set; }
    public string Status { get; set; } = string.Empty;
    public int BatteryLevel { get; set; }
    public bool IsCleaning { get; set; }
    public int CleaningProgress { get; set; }
    public int? EstimatedTimeRemaining { get; set; }
    public DateTime LastUpdateTime { get; set; }
}