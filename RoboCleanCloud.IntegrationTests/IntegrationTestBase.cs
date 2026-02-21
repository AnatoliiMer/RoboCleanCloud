using Microsoft.AspNetCore.Mvc.Testing;
using RoboCleanCloud.IntegrationTests.Fixtures;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace RoboCleanCloud.IntegrationTests;

public abstract class IntegrationTestBase : IClassFixture<ApiWebApplicationFactory>
{
    protected readonly HttpClient _client;
    protected readonly ApiWebApplicationFactory _factory;

    protected IntegrationTestBase(ApiWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    protected async Task AuthenticateAsync()
    {
        var token = await GetAuthToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
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
}