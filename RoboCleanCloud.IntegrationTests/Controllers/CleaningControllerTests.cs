using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using RoboCleanCloud.Application.DTOs;
using RoboCleanCloud.Application.UseCases.Cleaning.Commands;
using RoboCleanCloud.Application.UseCases.Robots.Commands;
using RoboCleanCloud.IntegrationTests.Fixtures;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace RoboCleanCloud.IntegrationTests.Controllers;

public class CleaningControllerTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly ApiWebApplicationFactory _factory;

    public CleaningControllerTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("http://localhost:5180")
        });
    }

    private async Task<string> GetAuthToken()
    {
        var loginRequest = new
        {
            UserId = Guid.NewGuid(),
            Email = "test@example.com",
            Name = "Test User"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/token", loginRequest);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<AuthTokenResponse>();
        return result!.Token;
    }

    private class AuthTokenResponse
    {
        public string Token { get; set; } = null!;
        public DateTime Expiration { get; set; }
    }

    private async Task<RobotResponse> CreateTestRobot()
    {
        var request = new
        {
            serialNumber = $"TEST-{Guid.NewGuid():N}",
            model = "X1000",
            friendlyName = "Test Robot",
            ownerId = Guid.NewGuid().ToString(),
            wifiSsid = "TestWiFi",
            wifiPassword = "test123"
        };

        var response = await _client.PostAsJsonAsync("/api/v1/robots", request);
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<RobotResponse>())!;
    }

    [Fact]
    public async Task StartCleaning_WithValidRobot_ShouldReturnAccepted()
    {
        // Arrange
        var token = await GetAuthToken();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var robot = await CreateTestRobot();

        var command = new
        {
            robotId = robot.RobotId,
            mode = "Full",
            zoneIds = new[] { Guid.NewGuid() },
            isScheduled = false
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/cleaning/start", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<CleaningSessionResponse>();
            result.Should().NotBeNull();
            result!.RobotId.Should().Be(robot.RobotId);
            result.Mode.ToString().Should().Be("Full");
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Request failed: {response.StatusCode}, Error: {error}");
        }
    }

    [Fact]
    public async Task GetCleaningHistory_ShouldReturnPagedResult()
    {
        // Arrange
        var token = await GetAuthToken();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var robot = await CreateTestRobot();

        // Создаем несколько сессий уборки
        for (int i = 0; i < 3; i++)
        {
            var command = new
            {
                robotId = robot.RobotId,
                mode = "Full",
                zoneIds = new[] { Guid.NewGuid() },
                isScheduled = false
            };
            await _client.PostAsJsonAsync("/api/v1/cleaning/start", command);
        }

        // Act
        var response = await _client.GetAsync($"/api/v1/cleaning/sessions?robotId={robot.RobotId}&page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<PagedResult<CleaningSessionDto>>();
            result.Should().NotBeNull();
            result!.Items.Should().NotBeEmpty();
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Request failed: {response.StatusCode}, Error: {error}");
        }
    }

    [Fact]
    public async Task StopCleaning_WithActiveSession_ShouldReturnAccepted()
    {
        // Arrange
        var token = await GetAuthToken();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var robot = await CreateTestRobot();

        var startCommand = new
        {
            robotId = robot.RobotId,
            mode = "Full",
            zoneIds = new[] { Guid.NewGuid() },
            isScheduled = false
        };

        await _client.PostAsJsonAsync("/api/v1/cleaning/start", startCommand);

        // Act
        var response = await _client.PostAsync($"/api/v1/cleaning/{robot.RobotId}/stop", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
    }
}