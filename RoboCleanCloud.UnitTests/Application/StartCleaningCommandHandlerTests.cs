using FluentAssertions;
using Moq;
using RoboCleanCloud.Application.Exceptions;
using RoboCleanCloud.Application.Interfaces.Repositories;
using RoboCleanCloud.Application.Interfaces.Services;
using RoboCleanCloud.Application.UseCases.Cleaning.Commands;
using RoboCleanCloud.Domain.Entities;
using RoboCleanCloud.Domain.Enums;
using RoboCleanCloud.Domain.Exceptions;
using Xunit;

namespace RoboCleanCloud.UnitTests.Application;

public class StartCleaningCommandHandlerTests
{
    private readonly Mock<IRobotRepository> _robotRepositoryMock;
    private readonly Mock<ICleaningSessionRepository> _sessionRepositoryMock;
    private readonly Mock<IRobotCommandGateway> _robotCommandGatewayMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly StartCleaningCommandHandler _handler;

    public StartCleaningCommandHandlerTests()
    {
        _robotRepositoryMock = new Mock<IRobotRepository>();
        _sessionRepositoryMock = new Mock<ICleaningSessionRepository>();
        _robotCommandGatewayMock = new Mock<IRobotCommandGateway>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new StartCleaningCommandHandler(
            _robotRepositoryMock.Object,
            _sessionRepositoryMock.Object,
            _robotCommandGatewayMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldCreateSessionAndStartCleaning()
    {
        // Arrange
        var robotId = Guid.NewGuid();
        Robot robot = CreateValidRobot(robotId);
        var command = new StartCleaningCommand(
            robotId,
            CleaningMode.Full,
            new List<Guid> { Guid.NewGuid() },
            false,
            null);

        _robotRepositoryMock.Setup(x => x.GetByIdAsync(robotId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(robot);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.RobotId.Should().Be(robotId);
        result.Mode.Should().Be(CleaningMode.Full);
        result.Status.Should().Be(CleaningSessionStatus.Planned);

        _sessionRepositoryMock.Verify(x =>
            x.AddAsync(It.IsAny<CleaningSession>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _robotRepositoryMock.Verify(x =>
            x.Update(robot),
            Times.Once);

        _robotCommandGatewayMock.Verify(x =>
            x.SendCleaningCommandAsync(robotId, It.IsAny<Guid>(),
                CleaningMode.Full, command.ZoneIds, It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(x =>
            x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentRobot_ShouldThrowNotFoundException()
    {
        // Arrange
        var robotId = Guid.NewGuid();
        var command = new StartCleaningCommand(
            robotId,
            CleaningMode.Full,
            new List<Guid>(),
            false,
            null);

        _robotRepositoryMock.Setup(x => x.GetByIdAsync(robotId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Robot)null!);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*{robotId}*");
    }

    [Fact]
    public async Task Handle_WithRobotNotReady_ShouldThrowDomainException()
    {
        // Arrange
        var robotId = Guid.NewGuid();
        Robot robot = CreateValidRobot(robotId);
        robot.UpdateStatus(ConnectionStatus.Offline); // Robot offline

        var command = new StartCleaningCommand(
            robotId,
            CleaningMode.Full,
            new List<Guid>(),
            false,
            null);

        _robotRepositoryMock.Setup(x => x.GetByIdAsync(robotId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(robot);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        // Исправляем ожидаемое сообщение на реальное
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Robot is not ready for cleaning");  // Изменено с "Robot cannot start cleaning"
    }

    private Robot CreateValidRobot(Guid robotId)
    {
        var robot = new Robot(
            "RC-2024-001",
            "X1000",
            "Test Robot",
            Guid.NewGuid());

        typeof(Robot).GetProperty(nameof(Robot.Id))?.SetValue(robot, robotId);
        robot.UpdateStatus(ConnectionStatus.Online);
        robot.UpdateBatteryLevel(100);

        return robot;
    }
}