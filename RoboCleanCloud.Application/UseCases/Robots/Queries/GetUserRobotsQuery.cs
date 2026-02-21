using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RoboCleanCloud.Application.DTOs;
using RoboCleanCloud.Application.Interfaces.Repositories;

namespace RoboCleanCloud.Application.UseCases.Robots.Queries;

public record GetUserRobotsQuery(Guid UserId, int Page = 1, int PageSize = 10) : IRequest<PagedResult<RobotDto>>;

public class GetUserRobotsQueryHandler : IRequestHandler<GetUserRobotsQuery, PagedResult<RobotDto>>
{
    private readonly IRobotRepository _robotRepository;

    public GetUserRobotsQueryHandler(IRobotRepository robotRepository)
    {
        _robotRepository = robotRepository;
    }

    public async Task<PagedResult<RobotDto>> Handle(GetUserRobotsQuery request, CancellationToken cancellationToken)
    {
        var robots = await _robotRepository.GetByOwnerIdAsync(request.UserId, cancellationToken);

        var items = robots.Select(r => new RobotDto
        {
            Id = r.Id,
            SerialNumber = r.SerialNumber,
            Model = r.Model,
            FriendlyName = r.FriendlyName,
            OwnerId = r.OwnerId,
            ConnectionStatus = r.ConnectionStatus,
            BatteryLevel = r.BatteryLevel,
            FirmwareVersion = r.FirmwareVersion,
            RegisteredAt = r.RegisteredAt,
            LastSeenAt = r.LastSeenAt,
            DustbinLevel = r.DustbinLevel
        }).ToList();

        return new PagedResult<RobotDto>
        {
            Items = items,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = items.Count
        };
    }
}