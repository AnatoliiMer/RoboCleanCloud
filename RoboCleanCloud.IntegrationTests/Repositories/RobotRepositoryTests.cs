using FluentAssertions;
using RoboCleanCloud.Domain.Entities;
using RoboCleanCloud.Domain.Enums;
using RoboCleanCloud.Infrastructure.Persistence.Repositories;
using RoboCleanCloud.IntegrationTests.Fixtures;
using Xunit;

namespace RoboCleanCloud.IntegrationTests.Repositories;

[Collection("Database")]
public class RobotRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    private readonly RobotRepository _repository;

    public RobotRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _repository = new RobotRepository(_fixture.Context);
    }

    [Fact]
    public async Task AddAsync_ShouldPersistRobot()
    {
        // Arrange
        var robot = new Robot(
            "RC-INT-001",
            "X1000",
            "Integration Test Robot",
            Guid.NewGuid());

        // Act
        await _repository.AddAsync(robot);
        await _fixture.Context.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(robot.Id);
        retrieved.Should().NotBeNull();
        retrieved!.SerialNumber.Should().Be(robot.SerialNumber);
        retrieved.FriendlyName.Should().Be(robot.FriendlyName);
    }

    [Fact]
    public async Task GetByOwnerIdAsync_ShouldReturnUserRobots()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var robot1 = new Robot("RC-INT-002", "X1000", "Robot 1", ownerId);
        var robot2 = new Robot("RC-INT-003", "X1000", "Robot 2", ownerId);
        var robot3 = new Robot("RC-INT-004", "X1000", "Robot 3", Guid.NewGuid());

        await _repository.AddAsync(robot1);
        await _repository.AddAsync(robot2);
        await _repository.AddAsync(robot3);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var userRobots = await _repository.GetByOwnerIdAsync(ownerId);

        // Assert
        userRobots.Should().HaveCount(2);
        userRobots.Should().Contain(r => r.Id == robot1.Id);
        userRobots.Should().Contain(r => r.Id == robot2.Id);
        userRobots.Should().NotContain(r => r.Id == robot3.Id);
    }

    [Fact]
    public async Task GetOnlineRobotsAsync_ShouldReturnOnlyOnlineRobots()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var robot1 = new Robot("RC-INT-005", "X1000", "Online Robot", ownerId);
        robot1.UpdateStatus(ConnectionStatus.Online);

        var robot2 = new Robot("RC-INT-006", "X1000", "Busy Robot", ownerId);
        robot2.UpdateStatus(ConnectionStatus.Busy);

        var robot3 = new Robot("RC-INT-007", "X1000", "Offline Robot", ownerId);
        robot3.UpdateStatus(ConnectionStatus.Offline);

        await _repository.AddAsync(robot1);
        await _repository.AddAsync(robot2);
        await _repository.AddAsync(robot3);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var onlineRobots = await _repository.GetOnlineRobotsAsync();

        // Assert
        onlineRobots.Should().HaveCount(2);
        onlineRobots.Should().Contain(r => r.Id == robot1.Id);
        onlineRobots.Should().Contain(r => r.Id == robot2.Id);
        onlineRobots.Should().NotContain(r => r.Id == robot3.Id);
    }

    [Fact]
    public async Task Update_ShouldModifyRobot()
    {
        // Arrange
        var robot = new Robot("RC-INT-008", "X1000", "Original Name", Guid.NewGuid());
        await _repository.AddAsync(robot);
        await _fixture.Context.SaveChangesAsync();

        var newName = "Updated Name";
        typeof(Robot).GetProperty(nameof(Robot.FriendlyName))!
            .SetValue(robot, newName);

        // Act
        _repository.Update(robot);
        await _fixture.Context.SaveChangesAsync();

        // Assert
        var updated = await _repository.GetByIdAsync(robot.Id);
        updated!.FriendlyName.Should().Be(newName);
    }

    [Fact]
    public async Task Delete_ShouldRemoveRobot()
    {
        // Arrange
        var robot = new Robot("RC-INT-009", "X1000", "To Delete", Guid.NewGuid());
        await _repository.AddAsync(robot);
        await _fixture.Context.SaveChangesAsync();

        // Act
        _repository.Delete(robot);
        await _fixture.Context.SaveChangesAsync();

        // Assert
        var deleted = await _repository.GetByIdAsync(robot.Id);
        deleted.Should().BeNull();
    }
}