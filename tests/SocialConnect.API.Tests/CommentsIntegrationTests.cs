using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Common.Models;
using SocialConnect.Application.Features.Auth.Commands.Login;
using SocialConnect.Application.Features.Auth.Commands.Register;
using SocialConnect.Application.Features.Comments.Commands.AddComment;
using SocialConnect.Application.Features.Posts.Commands.CreatePost;
using SocialConnect.Domain.Enums;
using SocialConnect.Infrastructure.Persistence;

namespace SocialConnect.API.Tests;

public class CommentsIntegrationTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;

    public CommentsIntegrationTests(IntegrationTestWebAppFactory factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                var fileStorageMock = new Mock<IFileStorageService>();
                fileStorageMock
                    .Setup(x => x.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Stream stream, string name, string contentType, CancellationToken ct) => $"http://fake-storage.local/{name}");

                services.RemoveAll<IFileStorageService>();
                services.AddSingleton<IFileStorageService>(fileStorageMock.Object);
            });
        });
        _client = _factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
    }

    private async Task<string> AuthenticateAsync(string email, string username)
    {
        var registerCommand = new RegisterCommand(email, "Password123!", username, "First", "Last");
        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", registerCommand);

        AuthResult? authResult;
        if (registerResponse.IsSuccessStatusCode)
        {
            var res = await registerResponse.Content.ReadFromJsonAsync<ApiResponse<AuthResult>>();
            authResult = res?.Data;
        }
        else
        {
            var loginCommand = new LoginCommand(email, "Password123!");
            var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", loginCommand);
            var res = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<AuthResult>>();
            authResult = res?.Data;
        }

        return authResult?.Token ?? string.Empty;
    }

    [Fact]
    public async Task CommentLifecycle_ShouldCreateUpdateAndDeleteSuccessfully()
    {
        // Arrange
        var token = await AuthenticateAsync("commentlife@example.com", "commentlife_user");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // 1. Create a post
        var createPostCommand = new CreatePostCommand("Post Content", PostType.Text, PostStatus.Published, null);
        var postResponse = await _client.PostAsJsonAsync("/api/v1/posts", createPostCommand);
        postResponse.EnsureSuccessStatusCode();
        var postResult = await postResponse.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
        var postId = postResult!.Data;

        // 2. Add a comment
        var addCommentCommand = new AddCommentCommand(postId, "Initial Comment Content", null);
        var commentResponse = await _client.PostAsJsonAsync("/api/v1/comments", addCommentCommand);
        commentResponse.EnsureSuccessStatusCode();
        var commentResult = await commentResponse.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
        var commentId = commentResult!.Data;

        // 3. Update the comment
        var updateResponse = await _client.PutAsJsonAsync($"/api/v1/comments/{commentId}", "Updated Comment Content");
        updateResponse.EnsureSuccessStatusCode();
        var updateResult = await updateResponse.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        updateResult!.Success.Should().BeTrue();

        // 4. Delete the comment
        var deleteResponse = await _client.DeleteAsync($"/api/v1/comments/{commentId}");
        deleteResponse.EnsureSuccessStatusCode();
        var deleteResult = await deleteResponse.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        deleteResult!.Success.Should().BeTrue();
    }
}
