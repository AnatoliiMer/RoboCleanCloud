using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoboCleanCloud.Api.Extensions;
using RoboCleanCloud.Application.DTOs;
using RoboCleanCloud.Application.UseCases.Robots.Commands;
using RoboCleanCloud.Application.UseCases.Robots.Queries;

namespace RoboCleanCloud.Api.Controllers.V1;

[ApiController]
[Route("api/v1/robots")]
[Authorize] //временно отключаем//
public class RobotsController : ControllerBase
{
    private readonly IMediator _mediator;

    public RobotsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Регистрация нового робота в системе
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(RobotResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<RobotResponse>> RegisterRobot(
        [FromBody] RegisterRobotCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetRobot), new { id = result.RobotId }, result);
    }

    /// <summary>
    /// Получение информации о роботе
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RobotDetailsDto), StatusCodes.Status200OK)] // Изменено с RobotDetailsResponse на RobotDetailsDto
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RobotDetailsDto>> GetRobot(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetRobotQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Получение списка роботов пользователя
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<RobotDto>), StatusCodes.Status200OK)] // Изменено с RobotResponse на RobotDto
    public async Task<ActionResult<PagedResult<RobotDto>>> GetUserRobots(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId(); // Extension method
        var query = new GetUserRobotsQuery(userId, page, pageSize);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Обновление информации о роботе
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateRobot(
        Guid id,
        [FromBody] UpdateRobotCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.RobotId)
            return BadRequest("ID mismatch");

        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Удаление робота из системы
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRobot(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteRobotCommand(id);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}