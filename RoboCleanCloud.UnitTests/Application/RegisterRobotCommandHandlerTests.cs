using FluentAssertions;
using Moq;
using RoboCleanCloud.Application.Exceptions;
using RoboCleanCloud.Application.Interfaces.Repositories;
using RoboCleanCloud.Application.Interfaces.Services;
using RoboCleanCloud.Application.UseCases.Robots.Commands;
using RoboCleanCloud.Domain.Entities;
using RoboCleanCloud.Domain.Exceptions;
using Xunit;

namespace RoboCleanCloud.UnitTests.Application;

public class RegisterRobotCommandHandlerTests
{
    private readonly Mock<IRobotRepository> _robotRepositoryMock;
    private readonly Mock<IWifiProvisioningService> _wifiServiceMock;
    private readonly Mock<IVendorApiClient> _vendorApiClientMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly RegisterRobotCommandHandler _handler;

    public RegisterRobotCommandHandlerTests()
    {
        _robotRepositoryMock = new Mock<IRobotRepository>();
        _wifiServiceMock = new Mock<IWifiProvisioningService>();
        _vendorApiClientMock = new Mock<IVendorApiClient>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new RegisterRobotCommandHandler(
            _robotRepositoryMock.Object,
            _wifiServiceMock.Object,
            _vendorApiClientMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldRegisterRobot()
    {
        // Arrange
        var command = new RegisterRobotCommand(
            SerialNumber: "RC-2024-001",
            Model: "X1000",
            FriendlyName: "Test Robot",
            OwnerId: Guid.NewGuid(),
            WifiSsid: "HomeWiFi",
            WifiPassword: "password123");

        _robotRepositoryMock.Setup(x => x.ExistsBySerialNumberAsync(
                command.SerialNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _vendorApiClientMock.Setup(x => x.ValidateSerialNumberAsync(
                command.SerialNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.SerialNumber.Should().Be(command.SerialNumber);
        result.FriendlyName.Should().Be(command.FriendlyName);

        _robotRepositoryMock.Verify(x => x.AddAsync(
            It.Is<Robot>(r => r.SerialNumber == command.SerialNumber),
            It.IsAny<CancellationToken>()), Times.Once);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithExistingRobot_ShouldThrowDomainException()
    {
        // Arrange
        var command = new RegisterRobotCommand(
            "RC-2024-001",
            "X1000",
            "Test Robot",
            Guid.NewGuid(),
            "HomeWiFi",
            "password123");

        _robotRepositoryMock.Setup(x => x.ExistsBySerialNumberAsync(
                command.SerialNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage($"*{command.SerialNumber}*");
    }

    [Fact]
    public async Task Handle_WithInvalidSerialNumber_ShouldThrowDomainException()
    {
        // Arrange
        var command = new RegisterRobotCommand(
            "INVALID-001",
            "X1000",
            "Test Robot",
            Guid.NewGuid(),
            "HomeWiFi",
            "password123");

        _robotRepositoryMock.Setup(x => x.ExistsBySerialNumberAsync(
                command.SerialNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _vendorApiClientMock.Setup(x => x.ValidateSerialNumberAsync(
                command.SerialNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Invalid serial number");
    }
}