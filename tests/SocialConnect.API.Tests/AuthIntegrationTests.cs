using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SocialConnect.Application.Common.Models;
using SocialConnect.Application.Features.Auth.Commands.Login;
using SocialConnect.Application.Features.Auth.Commands.Register;
using SocialConnect.Infrastructure.Persistence;

namespace SocialConnect.API.Tests;

public class AuthIntegrationTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly HttpClient _client;
    private readonly IntegrationTestWebAppFactory _factory;

    public AuthIntegrationTests(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();

        // Ensure database is created and migrations are applied
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
    }

    [Fact]
    public async Task Register_WithValidData_ShouldReturnToken()
    {
        // Arrange
        var command = new RegisterCommand("newuser@example.com", "Password123!", "newuser", "New", "User");

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", command);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResult>>();

        result.Should().NotBeNull();
        var data = result?.Data;
        data.Should().NotBeNull();
        data?.Token.Should().NotBeNullOrEmpty();
        data?.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnToken()
    {
        // Arrange
        var registerCommand = new RegisterCommand("loginuser@example.com", "Password123!", "loginuser", "Login", "User");
        await _client.PostAsJsonAsync("/api/v1/auth/register", registerCommand);

        var loginCommand = new LoginCommand("loginuser@example.com", "Password123!");

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginCommand);

        // Assert
        // Assert
        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            throw new Exception($"HTTP {response.StatusCode}: {content}");
        }
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResult>>();

        result.Should().NotBeNull();
        var data = result?.Data;
        data.Should().NotBeNull();
        data?.Token.Should().NotBeNullOrEmpty();
        data?.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
    {
        // Arrange
        var loginCommand = new LoginCommand("nonexistent@example.com", "WrongPassword123!");

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginCommand);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }
}
