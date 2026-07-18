using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SocialConnect.Application.Common.Models;
using SocialConnect.Application.Features.Auth.Commands.Register;
using SocialConnect.Application.Features.Posts.Commands.CreatePost;
using SocialConnect.Domain.Enums;
using SocialConnect.Infrastructure.Persistence;

namespace SocialConnect.API.Tests;

public class AuditLoggingIntegrationTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly HttpClient _client;
    private readonly IntegrationTestWebAppFactory _factory;

    public AuditLoggingIntegrationTests(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();

        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
    }

    [Fact]
    public async Task CreatePost_ShouldGenerateAuditLogAutomatically()
    {
        // Arrange
        var email = $"auditor_{Guid.NewGuid().ToString("N").Substring(0, 15)}@example.com";
        var username = $"aud_usr_{Guid.NewGuid().ToString("N").Substring(0, 15)}";

        // 1. Register User
        var registerCommand = new RegisterCommand(email, "Password123!", username, "Audit", "User");
        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", registerCommand);
        registerResponse.EnsureSuccessStatusCode();
        var registerResult = await registerResponse.Content.ReadFromJsonAsync<ApiResponse<AuthResult>>();
        var token = registerResult!.Data!.Token;

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Clear existing audit logs for test isolation if needed, or query specifically for our user
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await context.Users.FirstAsync(u => u.UserName == username);

        // 2. Create a Post
        var createPostCommand = new CreatePostCommand("Testing Audit Logging", PostType.Text, PostStatus.Published, null);
        var postResponse = await _client.PostAsJsonAsync("/api/v1/posts", createPostCommand);
        postResponse.EnsureSuccessStatusCode();
        var postResult = await postResponse.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
        var postId = postResult!.Data;

        // 3. Query the AuditLogs from the database context directly
        var auditLogs = await context.AuditLogs
            .Where(a => a.UserId == user.Id && a.EntityType == "Post")
            .ToListAsync();

        // Assert
        auditLogs.Should().NotBeEmpty();
        var audit = auditLogs.First();
        audit.Action.Should().Be("Added");
        audit.EntityType.Should().Be("Post");
        audit.EntityId.Should().Be(postId.ToString());
        audit.NewValues.Should().Contain("Testing Audit Logging");
        audit.OldValues.Should().BeNull();
    }
}
