using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoboCleanCloud.Application.UseCases.Scheduling.Commands;
using RoboCleanCloud.Application.UseCases.Scheduling.Queries;

namespace RoboCleanCloud.Api.Controllers.V1;

[ApiController]
[Route("api/v1/schedules")]
[Authorize]  //временно отключаем//
public class SchedulesController : ControllerBase
{
    private readonly IMediator _mediator;

    public SchedulesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Создание нового расписания уборок
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ScheduleResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ScheduleResponse>> CreateSchedule(
        [FromBody] CreateScheduleCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetSchedule), new { id = result.ScheduleId }, result);
    }

    /// <summary>
    /// Получение расписания по ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ScheduleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ScheduleResponse>> GetSchedule(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetScheduleQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Получение всех расписаний для робота
    /// </summary>
    [HttpGet("robot/{robotId:guid}")]
    [ProducesResponseType(typeof(List<ScheduleResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ScheduleResponse>>> GetRobotSchedules(
        Guid robotId,
        CancellationToken cancellationToken)
    {
        var query = new GetRobotSchedulesQuery(robotId);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Обновление расписания
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateSchedule(
        Guid id,
        [FromBody] UpdateScheduleCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.ScheduleId)
            return BadRequest("ID mismatch");

        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Удаление расписания
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSchedule(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteScheduleCommand(id);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Активация/деактивация расписания
    /// </summary>
    [HttpPatch("{id:guid}/toggle")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ToggleSchedule(
        Guid id,
        [FromBody] ToggleScheduleCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.ScheduleId)
            return BadRequest("ID mismatch");

        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}