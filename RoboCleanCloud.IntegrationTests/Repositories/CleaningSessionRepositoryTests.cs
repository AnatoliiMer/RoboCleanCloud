using FluentAssertions;
using RoboCleanCloud.Domain.Entities;
using RoboCleanCloud.Domain.Enums;
using RoboCleanCloud.Infrastructure.Persistence.Repositories;
using RoboCleanCloud.IntegrationTests.Fixtures;
using Xunit;

namespace RoboCleanCloud.IntegrationTests.Repositories;

[Collection("Database")]
public class CleaningSessionRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    private readonly CleaningSessionRepository _repository;
    private readonly RobotRepository _robotRepository;

    public CleaningSessionRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _repository = new CleaningSessionRepository(_fixture.Context);
        _robotRepository = new RobotRepository(_fixture.Context);
    }

    [Fact]
    public async Task AddAsync_ShouldPersistSession()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();

        var robot = new Robot("RC-SESS-001", "X1000", "Test Robot", Guid.NewGuid());
        await _robotRepository.AddAsync(robot);
        await _fixture.Context.SaveChangesAsync();

        var session = new CleaningSession(
            robot.Id,
            CleaningMode.Full,
            new List<Guid> { Guid.NewGuid() });

        // Act
        await _repository.AddAsync(session);
        await _fixture.Context.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(session.Id);
        retrieved.Should().NotBeNull();
        retrieved!.RobotId.Should().Be(robot.Id);
        retrieved.Mode.Should().Be(CleaningMode.Full);
    }

    [Fact]
    public async Task GetByRobotIdAsync_ShouldReturnRobotSessions()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();

        var robot = new Robot("RC-SESS-002", "X1000", "Test Robot", Guid.NewGuid());
        await _robotRepository.AddAsync(robot);
        await _fixture.Context.SaveChangesAsync();

        var session1 = new CleaningSession(robot.Id, CleaningMode.Full, null);
        var session2 = new CleaningSession(robot.Id, CleaningMode.Quick, null);
        var session3 = new CleaningSession(robot.Id, CleaningMode.Spot, null);

        await _repository.AddAsync(session1);
        await _repository.AddAsync(session2);
        await _repository.AddAsync(session3);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var sessions = await _repository.GetByRobotIdAsync(robot.Id);

        // Assert
        sessions.Should().HaveCount(3);
        sessions.Should().Contain(s => s.Id == session1.Id);
        sessions.Should().Contain(s => s.Id == session2.Id);
        sessions.Should().Contain(s => s.Id == session3.Id);
    }

    [Fact]
    public async Task GetActiveSessionAsync_ShouldReturnInProgressSession()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();

        var robot = new Robot("RC-SESS-003", "X1000", "Test Robot", Guid.NewGuid());
        await _robotRepository.AddAsync(robot);
        await _fixture.Context.SaveChangesAsync();

        var activeSession = new CleaningSession(robot.Id, CleaningMode.Full, null);
        activeSession.Start();

        var completedSession = new CleaningSession(robot.Id, CleaningMode.Quick, null);
        completedSession.Start();
        completedSession.Complete(10, 5);

        await _repository.AddAsync(activeSession);
        await _repository.AddAsync(completedSession);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetActiveSessionAsync(robot.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(activeSession.Id);
        result.Status.Should().Be(CleaningSessionStatus.InProgress);
    }

    [Fact]
    public async Task GetByDateRangeAsync_ShouldReturnSessionsInRange()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();

        var robot = new Robot("RC-SESS-004", "X1000", "Test Robot", Guid.NewGuid());
        await _robotRepository.AddAsync(robot);
        await _fixture.Context.SaveChangesAsync();

        var now = DateTime.UtcNow;

        // Создаем сессии с разными датами
        var session1 = new CleaningSession(robot.Id, CleaningMode.Full, null);
        typeof(CleaningSession).GetProperty(nameof(CleaningSession.StartedAt))!
            .SetValue(session1, now.AddDays(-2));

        var session2 = new CleaningSession(robot.Id, CleaningMode.Full, null);
        typeof(CleaningSession).GetProperty(nameof(CleaningSession.StartedAt))!
            .SetValue(session2, now.AddDays(-1));

        var session3 = new CleaningSession(robot.Id, CleaningMode.Full, null);
        typeof(CleaningSession).GetProperty(nameof(CleaningSession.StartedAt))!
            .SetValue(session3, now.AddDays(1));

        await _repository.AddAsync(session1);
        await _repository.AddAsync(session2);
        await _repository.AddAsync(session3);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var from = now.AddDays(-3);
        var to = now;
        var sessions = await _repository.GetByDateRangeAsync(from, to);

        // Assert
        sessions.Should().HaveCount(2);
        sessions.Should().Contain(s => s.Id == session1.Id);
        sessions.Should().Contain(s => s.Id == session2.Id);
        sessions.Should().NotContain(s => s.Id == session3.Id);
    }
}