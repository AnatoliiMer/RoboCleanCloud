using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using RoboCleanCloud.Application.DTOs;
using RoboCleanCloud.Application.UseCases.Robots.Commands;
using RoboCleanCloud.IntegrationTests.Fixtures;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace RoboCleanCloud.IntegrationTests.Controllers;

public class RobotsControllerTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly ApiWebApplicationFactory _factory;

    public RobotsControllerTests(ApiWebApplicationFactory factory)
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

    [Fact]
    public async Task RegisterRobot_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var token = await GetAuthToken();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var request = new
        {
            serialNumber = $"TEST-API-{Guid.NewGuid():N}",
            model = "X1000",
            friendlyName = "API Test Robot",
            ownerId = Guid.NewGuid().ToString(),
            wifiSsid = "TestWiFi",
            wifiPassword = "test123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/robots", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        if (response.IsSuccessStatusCode)
        {
            var robot = await response.Content.ReadFromJsonAsync<RobotResponse>();
            robot.Should().NotBeNull();
            robot!.SerialNumber.Should().Be(request.serialNumber);
            robot.FriendlyName.Should().Be(request.friendlyName);
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Request failed: {response.StatusCode}, Error: {error}");
        }
    }

    [Fact]
    public async Task GetRobot_WithExistingId_ShouldReturnRobot()
    {
        // Arrange - сначала создаем робота
        var token = await GetAuthToken();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var createRequest = new
        {
            serialNumber = $"TEST-API-{Guid.NewGuid():N}",
            model = "X1000",
            friendlyName = "Get Test Robot",
            ownerId = Guid.NewGuid().ToString(),
            wifiSsid = "TestWiFi",
            wifiPassword = "test123"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/v1/robots", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await createResponse.Content.ReadFromJsonAsync<RobotResponse>();
        created.Should().NotBeNull();

        // Act
        var response = await _client.GetAsync($"/api/v1/robots/{created!.RobotId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        if (response.IsSuccessStatusCode)
        {
            var robot = await response.Content.ReadFromJsonAsync<RobotDetailsDto>();
            robot.Should().NotBeNull();
            robot!.Id.Should().Be(created.RobotId);
            robot.FriendlyName.Should().Be(createRequest.friendlyName);
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Request failed: {response.StatusCode}, Error: {error}");
        }
    }

    [Fact]
    public async Task GetRobot_WithNonExistingId_ShouldReturnNotFound()
    {
        // Arrange
        var token = await GetAuthToken();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var nonExistingId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/v1/robots/{nonExistingId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetUserRobots_ShouldReturnList()
    {
        // Arrange
        var token = await GetAuthToken();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var ownerId = Guid.NewGuid();

        // Создаем несколько роботов для одного пользователя
        for (int i = 0; i < 3; i++)
        {
            var request = new
            {
                serialNumber = $"TEST-API-{Guid.NewGuid():N}",
                model = "X1000",
                friendlyName = $"Robot {i}",
                ownerId = ownerId.ToString(),
                wifiSsid = "TestWiFi",
                wifiPassword = "test123"
            };
            await _client.PostAsJsonAsync("/api/v1/robots", request);
        }

        // Act
        var response = await _client.GetAsync("/api/v1/robots?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<PagedResult<RobotDto>>();
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
    public async Task UpdateRobot_WithValidData_ShouldReturnNoContent()
    {
        // Arrange - создаем робота
        var token = await GetAuthToken();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var createRequest = new
        {
            serialNumber = $"TEST-API-{Guid.NewGuid():N}",
            model = "X1000",
            friendlyName = "Original Name",
            ownerId = Guid.NewGuid().ToString(),
            wifiSsid = "TestWiFi",
            wifiPassword = "test123"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/v1/robots", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await createResponse.Content.ReadFromJsonAsync<RobotResponse>();
        created.Should().NotBeNull();

        var updateRequest = new
        {
            robotId = created!.RobotId,
            friendlyName = "Updated Name"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/v1/robots/{created.RobotId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Проверяем, что имя обновилось
        var getResponse = await _client.GetAsync($"/api/v1/robots/{created.RobotId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var updated = await getResponse.Content.ReadFromJsonAsync<RobotDetailsDto>();
        updated!.FriendlyName.Should().Be("Updated Name");
    }

    [Fact]
    public async Task DeleteRobot_WithExistingId_ShouldReturnNoContent()
    {
        // Arrange - создаем робота
        var token = await GetAuthToken();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var createRequest = new
        {
            serialNumber = $"TEST-API-{Guid.NewGuid():N}",
            model = "X1000",
            friendlyName = "To Delete",
            ownerId = Guid.NewGuid().ToString(),
            wifiSsid = "TestWiFi",
            wifiPassword = "test123"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/v1/robots", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await createResponse.Content.ReadFromJsonAsync<RobotResponse>();
        created.Should().NotBeNull();

        // Act
        var response = await _client.DeleteAsync($"/api/v1/robots/{created!.RobotId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Проверяем, что робот удален
        var getResponse = await _client.GetAsync($"/api/v1/robots/{created.RobotId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}