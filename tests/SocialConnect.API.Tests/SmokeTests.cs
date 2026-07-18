using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using SocialConnect.Infrastructure.Persistence;
using FluentAssertions;

namespace SocialConnect.API.Tests;

public class SmokeTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly IntegrationTestWebAppFactory _factory;

    public SmokeTests(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ApplicationStartup_ShouldInitializeSuccessfully()
    {
        // Act
        var client = _factory.CreateClient();

        // Assert
        client.Should().NotBeNull();
    }

    [Fact]
    public void DependencyInjection_ShouldResolveControllersSuccessfully()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var services = scope.ServiceProvider;

        var controllerTypes = typeof(Program).Assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(ControllerBase)) && !t.IsAbstract);

        // Act & Assert
        foreach (var type in controllerTypes)
        {
            // Resolve each controller type to verify constructor injection dependencies are properly configured
            var instance = ActivatorUtilities.CreateInstance(services, type);
            instance.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task Database_ShouldBeHealthyAndConnectable()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Act
        var canConnect = await context.Database.CanConnectAsync();

        // Assert
        canConnect.Should().BeTrue();
    }
}
