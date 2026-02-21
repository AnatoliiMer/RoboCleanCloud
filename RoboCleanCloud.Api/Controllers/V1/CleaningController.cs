using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoboCleanCloud.Application.DTOs;
using RoboCleanCloud.Application.UseCases.Cleaning.Commands;
using RoboCleanCloud.Application.UseCases.Cleaning.Queries;

namespace RoboCleanCloud.Api.Controllers.V1;

[ApiController]
[Route("api/v1/cleaning")]
[Authorize] //временно отключаем//
public class CleaningController : ControllerBase
{
    private readonly IMediator _mediator;

    public CleaningController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Запуск немедленной уборки
    /// </summary>
    [HttpPost("start")]
    [ProducesResponseType(typeof(CleaningSessionResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CleaningSessionResponse>> StartCleaning(
        [FromBody] StartCleaningCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Accepted($"api/v1/cleaning/sessions/{result.SessionId}", result);
    }

    /// <summary>
    /// Остановка уборки
    /// </summary>
    [HttpPost("{robotId:guid}/stop")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> StopCleaning(
        Guid robotId,
        CancellationToken cancellationToken)
    {
        var command = new StopCleaningCommand(robotId);
        await _mediator.Send(command, cancellationToken);
        return Accepted();
    }

    /// <summary>
    /// Возврат робота на базу
    /// </summary>
    [HttpPost("{robotId:guid}/return-to-base")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> ReturnToBase(
        Guid robotId,
        CancellationToken cancellationToken)
    {
        var command = new ReturnToBaseCommand(robotId);
        await _mediator.Send(command, cancellationToken);
        return Accepted();
    }

    /// <summary>
    /// Получение истории уборок
    /// </summary>
    [HttpGet("sessions")]
    [ProducesResponseType(typeof(PagedResult<CleaningSessionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<CleaningSessionDto>>> GetCleaningHistory(
        [FromQuery] Guid? robotId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetCleaningHistoryQuery(robotId, from, to, page, pageSize);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Получение детальной информации о сессии уборки
    /// </summary>
    [HttpGet("sessions/{sessionId:guid}")]
    [ProducesResponseType(typeof(CleaningSessionDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CleaningSessionDetailsDto>> GetCleaningSession(
        Guid sessionId,
        CancellationToken cancellationToken)
    {
        var query = new GetCleaningSessionQuery(sessionId);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}