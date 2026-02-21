using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RoboCleanCloud.Application.Interfaces.Repositories;
using RoboCleanCloud.Application.Interfaces.Services;
using RoboCleanCloud.Domain.Entities;
using RoboCleanCloud.Domain.Exceptions;

namespace RoboCleanCloud.Application.UseCases.Robots.Commands;

public record RegisterRobotCommand(
    string SerialNumber,
    string Model,
    string FriendlyName,
    Guid OwnerId,
    string WifiSsid,
    string WifiPassword) : IRequest<RobotResponse>;

public record RobotResponse(
    Guid RobotId,
    string SerialNumber,
    string FriendlyName,
    string ConnectionStatus);

public class RegisterRobotCommandHandler : IRequestHandler<RegisterRobotCommand, RobotResponse>
{
    private readonly IRobotRepository _robotRepository;
    private readonly IWifiProvisioningService _wifiService;
    private readonly IVendorApiClient _vendorApiClient;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterRobotCommandHandler(
        IRobotRepository robotRepository,
        IWifiProvisioningService wifiService,
        IVendorApiClient vendorApiClient,
        IUnitOfWork unitOfWork)
    {
        _robotRepository = robotRepository;
        _wifiService = wifiService;
        _vendorApiClient = vendorApiClient;
        _unitOfWork = unitOfWork;
    }

    public async Task<RobotResponse> Handle(
        RegisterRobotCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Проверяем, не зарегистрирован ли робот
        // ИСПРАВЛЕНО: используем ExistsBySerialNumberAsync вместо ExistsAsync
        var exists = await _robotRepository.ExistsBySerialNumberAsync(request.SerialNumber, cancellationToken);
        if (exists)
            throw new DomainException($"Robot with serial number {request.SerialNumber} already registered");

        // 2. Проверяем серийный номер у производителя
        var isValid = await _vendorApiClient.ValidateSerialNumberAsync(
            request.SerialNumber,
            cancellationToken);

        if (!isValid)
            throw new DomainException("Invalid serial number");

        // 3. Создаем робота в системе
        var robot = new Robot(
            request.SerialNumber,
            request.Model,
            request.FriendlyName,
            request.OwnerId);

        await _robotRepository.AddAsync(robot, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 4. Асинхронно настраиваем WiFi (fire-and-forget)
        _ = Task.Run(async () =>
        {
            try
            {
                await _wifiService.ProvisionRobotAsync(
                    robot.Id,
                    request.WifiSsid,
                    request.WifiPassword,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                // Логируем ошибку, но не проваливаем регистрацию
                Console.WriteLine($"WiFi provisioning failed: {ex.Message}");
            }
        }, cancellationToken);

        return new RobotResponse(
            robot.Id,
            robot.SerialNumber,
            robot.FriendlyName,
            robot.ConnectionStatus.ToString());
    }
}