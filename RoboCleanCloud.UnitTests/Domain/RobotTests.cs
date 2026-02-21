using FluentAssertions;
using RoboCleanCloud.Domain.Entities;
using RoboCleanCloud.Domain.Enums;
using RoboCleanCloud.Domain.Exceptions;
using Xunit;

namespace RoboCleanCloud.UnitTests.Domain;

public class RobotTests
{
    [Fact]
    public void CreateRobot_WithValidParameters_ShouldInitializeCorrectly()
    {
        // Arrange
        var serialNumber = "RC-2024-001";
        var model = "X1000";
        var friendlyName = "Kitchen Cleaner";
        var ownerId = Guid.NewGuid();

        // Act
        var robot = new Robot(serialNumber, model, friendlyName, ownerId);

        // Assert
        robot.Should().NotBeNull();
        robot.SerialNumber.Should().Be(serialNumber);
        robot.Model.Should().Be(model);
        robot.FriendlyName.Should().Be(friendlyName);
        robot.OwnerId.Should().Be(ownerId);
        robot.ConnectionStatus.Should().Be(ConnectionStatus.Offline);
        robot.BatteryLevel.Should().Be(100);
        robot.FirmwareVersion.Should().Be("1.0.0");
        robot.RegisteredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData(-5)]
    [InlineData(150)]
    public void UpdateBatteryLevel_WithInvalidLevel_ShouldThrowDomainException(int invalidLevel)
    {
        // Arrange
        var robot = CreateValidRobot();

        // Act
        Action act = () => robot.UpdateBatteryLevel(invalidLevel);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Battery level must be between 0 and 100");
    }

    [Theory]
    [InlineData(100, ConnectionStatus.Online, true)]
    [InlineData(10, ConnectionStatus.Online, false)]
    [InlineData(100, ConnectionStatus.Busy, false)]
    [InlineData(100, ConnectionStatus.Offline, false)]
    [InlineData(100, ConnectionStatus.ReturningToBase, false)]
    public void CanStartCleaning_ShouldReturnExpectedResult(
        int batteryLevel,
        ConnectionStatus status,
        bool expected)
    {
        // Arrange
        var robot = CreateValidRobot();
        robot.UpdateBatteryLevel(batteryLevel);
        robot.UpdateStatus(status);

        // Act
        var result = robot.CanStartCleaning();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void StartCleaning_WhenRobotIsReady_ShouldUpdateStatusAndAddEvent()
    {
        // Arrange
        var robot = CreateValidRobot();
        robot.UpdateStatus(ConnectionStatus.Online);
        robot.UpdateBatteryLevel(100);

        // Важно: очищаем историю, которая могла быть создана при UpdateStatus
        robot.StatusHistory.Clear();

        // Act
        robot.StartCleaning();

        // Assert
        robot.ConnectionStatus.Should().Be(ConnectionStatus.Busy);
        robot.StatusHistory.Should().ContainSingle();

        var statusChange = robot.StatusHistory.First();
        statusChange.PreviousStatus.Should().Be(ConnectionStatus.Online);
        statusChange.NewStatus.Should().Be(ConnectionStatus.Busy);
        statusChange.Reason.Should().Be("Started cleaning");

        robot.DomainEvents.Should().Contain(e =>
            e.GetType().Name == "RobotStatusChangedEvent");
    }

    [Fact]
    public void UpdateStatus_WithReason_ShouldAddToHistory()
    {
        // Arrange
        var robot = CreateValidRobot();
        var reason = "Manual status change";

        // Act
        robot.UpdateStatus(ConnectionStatus.Online, reason);

        // Assert
        robot.StatusHistory.Should().ContainSingle();
        var history = robot.StatusHistory.First();
        history.PreviousStatus.Should().Be(ConnectionStatus.Offline);
        history.NewStatus.Should().Be(ConnectionStatus.Online);
        history.Reason.Should().Be(reason);
    }

    private Robot CreateValidRobot()
    {
        return new Robot(
            "RC-2024-001",
            "X1000",
            "Test Robot",
            Guid.NewGuid());
    }
}