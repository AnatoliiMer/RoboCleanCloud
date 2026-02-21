using FluentAssertions;
using RoboCleanCloud.Domain.Entities;
using RoboCleanCloud.Domain.Enums;
using RoboCleanCloud.Domain.Exceptions;
using Xunit;

namespace RoboCleanCloud.UnitTests.Domain;

public class CleaningSessionTests
{
    [Fact]
    public void CreateSession_WithValidParameters_ShouldInitializeCorrectly()
    {
        // Arrange
        var robotId = Guid.NewGuid();
        var mode = CleaningMode.Full;
        var zoneIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        // Act
        var session = new CleaningSession(robotId, mode, zoneIds);

        // Assert
        session.Should().NotBeNull();
        session.RobotId.Should().Be(robotId);
        session.Mode.Should().Be(mode);
        session.ZoneIds.Should().BeEquivalentTo(zoneIds);
        session.Status.Should().Be(CleaningSessionStatus.Planned);
        session.StartedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        session.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Start_WhenSessionIsPlanned_ShouldChangeStatusToInProgress()
    {
        // Arrange
        var session = CreateValidSession();

        // Act
        session.Start();

        // Assert
        session.Status.Should().Be(CleaningSessionStatus.InProgress);
    }

    [Fact]
    public void Start_WhenSessionAlreadyStarted_ShouldThrowDomainException()
    {
        // Arrange
        var session = CreateValidSession();
        session.Start();

        // Act
        Action act = () => session.Start();

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Session already started");
    }

    [Fact]
    public void Complete_WithValidData_ShouldUpdateStatusAndSetMetrics()
    {
        // Arrange
        var session = CreateValidSession();
        session.Start();
        var area = 45.5;
        var energy = 12.3;

        // Act
        session.Complete(area, energy);

        // Assert
        session.Status.Should().Be(CleaningSessionStatus.Completed);
        session.FinishedAt.Should().NotBeNull();
        session.AreaCleaned.Should().Be(area);
        session.EnergyConsumed.Should().Be(energy);
    }

    [Fact]
    public void Fail_WithError_ShouldAddErrorAndSetStatus()
    {
        // Arrange
        var session = CreateValidSession();
        var errorCode = "BRUSH_JAM";
        var errorMessage = "Main brush is stuck";

        // Act
        session.Fail(errorCode, errorMessage);

        // Assert
        session.Status.Should().Be(CleaningSessionStatus.Failed);
        session.FinishedAt.Should().NotBeNull();
        session.Errors.Should().ContainSingle();

        var error = session.Errors.First();
        error.ErrorCode.Should().Be(errorCode);
        error.Message.Should().Be(errorMessage);
    }

    [Fact]
    public void Pause_And_Resume_ShouldToggleStatus()
    {
        // Arrange
        var session = CreateValidSession();
        session.Start();

        // Act
        session.Pause();

        // Assert
        session.Status.Should().Be(CleaningSessionStatus.Paused);

        // Act
        session.Resume();

        // Assert
        session.Status.Should().Be(CleaningSessionStatus.InProgress);
    }

    [Fact]
    public void Cancel_ShouldSetStatusToCancelled()
    {
        // Arrange
        var session = CreateValidSession();
        session.Start();

        // Act
        session.Cancel();

        // Assert
        session.Status.Should().Be(CleaningSessionStatus.Cancelled);
        session.FinishedAt.Should().NotBeNull();
    }

    private CleaningSession CreateValidSession()
    {
        return new CleaningSession(
            Guid.NewGuid(),
            CleaningMode.Full,
            new List<Guid> { Guid.NewGuid() });
    }
}