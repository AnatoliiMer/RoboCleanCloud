using Microsoft.EntityFrameworkCore;
using RoboCleanCloud.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace RoboCleanCloud.IntegrationTests.Fixtures;

public class DatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer;
    public ApplicationDbContext Context { get; private set; } = null!;
    public string ConnectionString => _postgresContainer.GetConnectionString();

    public DatabaseFixture()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15")
            .WithDatabase("RoboCleanCloudTest")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .WithCleanUp(true)
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        Context = new ApplicationDbContext(options);
        await Context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await Context.DisposeAsync();
        await _postgresContainer.DisposeAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        await Context.Database.EnsureDeletedAsync();
        await Context.Database.MigrateAsync();
    }
}