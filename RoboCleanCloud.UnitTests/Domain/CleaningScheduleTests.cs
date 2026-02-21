using FluentAssertions;
using NCrontab;
using RoboCleanCloud.Domain.Entities;
using RoboCleanCloud.Domain.Enums;
using RoboCleanCloud.Domain.Exceptions;
using Xunit;

namespace RoboCleanCloud.UnitTests.Domain;

public class CleaningScheduleTests
{
    [Fact]
    public void CreateSchedule_WithValidCron_ShouldInitializeCorrectly()
    {
        // Arrange
        var robotId = Guid.NewGuid();
        var cronExpression = "0 10 * * 1-5"; // 10:00 по будням
        var mode = CleaningMode.Quick;
        var zoneIds = new List<Guid> { Guid.NewGuid() };

        // Act
        var schedule = new CleaningSchedule(robotId, cronExpression, mode, zoneIds);

        // Assert
        schedule.Should().NotBeNull();
        schedule.RobotId.Should().Be(robotId);
        schedule.CronExpression.Should().Be(cronExpression);
        schedule.Mode.Should().Be(mode);
        schedule.ZoneIds.Should().BeEquivalentTo(zoneIds);
        schedule.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData("invalid cron")]
    [InlineData("*/5 * * *")]
    public void CreateSchedule_WithInvalidCron_ShouldThrowDomainException(string invalidCron)
    {
        // Arrange
        var robotId = Guid.NewGuid();

        // Act
        Action act = () => new CleaningSchedule(robotId, invalidCron, CleaningMode.Quick, null);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*Invalid cron expression*");
    }

    [Fact]
    public void GetNextExecution_ForActiveSchedule_ShouldReturnFutureDate()
    {
        // Arrange
        var schedule = new CleaningSchedule(
            Guid.NewGuid(),
            "0 10 * * *", // Каждый день в 10:00
            CleaningMode.Full,
            null);

        // Act
        var next = schedule.GetNextExecution();

        // Assert
        next.Should().NotBeNull();
        next.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public void GetNextExecution_ForInactiveSchedule_ShouldReturnNull()
    {
        // Arrange
        var schedule = new CleaningSchedule(
            Guid.NewGuid(),
            "0 10 * * *",
            CleaningMode.Full,
            null);
        schedule.Deactivate();

        // Act
        var next = schedule.GetNextExecution();

        // Assert
        next.Should().BeNull();
    }

    [Fact]
    public void Activate_Deactivate_ShouldToggleIsActive()
    {
        // Arrange
        var schedule = CreateValidSchedule();

        // Assert - по умолчанию активен
        schedule.IsActive.Should().BeTrue();

        // Act
        schedule.Deactivate();

        // Assert
        schedule.IsActive.Should().BeFalse();

        // Act
        schedule.Activate();

        // Assert
        schedule.IsActive.Should().BeTrue();
    }

    [Fact]
    public void UpdateCron_WithValidExpression_ShouldUpdateAndValidate()
    {
        // Arrange
        var schedule = CreateValidSchedule();
        var newCron = "30 14 * * 6"; // Суббота в 14:30

        // Act
        schedule.UpdateCron(newCron);

        // Assert
        schedule.CronExpression.Should().Be(newCron);

        var next = schedule.GetNextExecution();
        next.Should().NotBeNull();  // Проверяем, что не null
        next!.Value.DayOfWeek.Should().Be(DayOfWeek.Saturday);
        next.Value.Hour.Should().Be(14);
        next.Value.Minute.Should().Be(30);
    }

    [Fact]
    public void SetQuietHours_WithValidHours_ShouldUpdate()
    {
        // Arrange
        var schedule = CreateValidSchedule();
        var startHour = 22;
        var endHour = 8;

        // Act
        schedule.SetQuietHours(startHour, endHour);

        // Assert
        schedule.QuietHoursStart.Should().Be(startHour);
        schedule.QuietHoursEnd.Should().Be(endHour);
    }

    [Theory]
    [InlineData(24, 8)]
    [InlineData(22, 25)]
    public void SetQuietHours_WithInvalidHours_ShouldThrowDomainException(int start, int end)
    {
        // Arrange
        var schedule = CreateValidSchedule();

        // Act
        Action act = () => schedule.SetQuietHours(start, end);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Quiet hours must be between 0 and 23");
    }

    [Fact]
    public void MarkTriggered_ShouldUpdateLastTriggeredAt()
    {
        // Arrange
        var schedule = CreateValidSchedule();

        // Act
        schedule.MarkTriggered();

        // Assert
        schedule.LastTriggeredAt.Should().NotBeNull();
        schedule.LastTriggeredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    private CleaningSchedule CreateValidSchedule()
    {
        return new CleaningSchedule(
            Guid.NewGuid(),
            "0 10 * * *",
            CleaningMode.Quick,
            null);
    }
}