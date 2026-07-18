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
using SocialConnect.Application.Features.Posts.Commands.CreatePost;
using SocialConnect.Application.Features.Posts.DTOs;
using SocialConnect.Application.Features.Users.DTOs;
using SocialConnect.Domain.Enums;
using SocialConnect.Infrastructure.Persistence;

namespace SocialConnect.API.Tests;

public class PostsIntegrationTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;

    public PostsIntegrationTests(IntegrationTestWebAppFactory factory)
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
    public async Task BlockAndMute_ShouldExcludePostsFromSearch()
    {
        // Arrange
        var tokenA = await AuthenticateAsync("usera@example.com", "user_a");
        var tokenB = await AuthenticateAsync("userb@example.com", "user_b");

        // 1. User B creates a post
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenB);
        var createPostCommand = new CreatePostCommand("Post from B containing unique_keyword_xxx", PostType.Text, PostStatus.Published, null);
        var postResponse = await _client.PostAsJsonAsync("/api/v1/posts", createPostCommand);
        postResponse.EnsureSuccessStatusCode();

        // Obtain User B's ID by calling /me while logged in as B
        var meResponse = await _client.GetAsync("/api/v1/users/me");
        var meResult = await meResponse.Content.ReadFromJsonAsync<ApiResponse<UserProfileDto>>();
        var userBId = meResult!.Data!.UserId;

        // 2. User A searches and finds the post
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
        var searchResponse = await _client.GetAsync("/api/v1/posts/search?searchTerm=unique_keyword_xxx");
        searchResponse.EnsureSuccessStatusCode();
        var searchResult = await searchResponse.Content.ReadFromJsonAsync<ApiResponse<PaginatedList<PostDto>>>();
        searchResult!.Data!.Items.Should().NotBeEmpty();

        // 3. User A blocks User B
        var blockResponse = await _client.PostAsync($"/api/v1/social/block/{userBId}", null);
        blockResponse.EnsureSuccessStatusCode();

        // 4. User A searches again - should not find User B's post
        var searchResponseAfterBlock = await _client.GetAsync("/api/v1/posts/search?searchTerm=unique_keyword_xxx");
        searchResponseAfterBlock.EnsureSuccessStatusCode();
        var searchResultAfterBlock = await searchResponseAfterBlock.Content.ReadFromJsonAsync<ApiResponse<PaginatedList<PostDto>>>();
        searchResultAfterBlock!.Data!.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task PostLifecycle_ShouldPublishAndScheduleSuccessfully()
    {
        // Arrange
        var token = await AuthenticateAsync("postlife@example.com", "postlife_user");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // 1. Create a draft post
        var createPostCommand = new CreatePostCommand("Draft Post Content", PostType.Text, PostStatus.Draft, null);
        var postResponse = await _client.PostAsJsonAsync("/api/v1/posts", createPostCommand);
        postResponse.EnsureSuccessStatusCode();
        var postResult = await postResponse.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
        var postId = postResult!.Data;

        // 2. Schedule the post
        var scheduleTime = DateTimeOffset.UtcNow.AddHours(2);
        var scheduleResponse = await _client.PostAsJsonAsync($"/api/v1/posts/{postId}/schedule", scheduleTime);
        scheduleResponse.EnsureSuccessStatusCode();

        // 3. Publish the post
        var publishResponse = await _client.PostAsync($"/api/v1/posts/{postId}/publish", null);
        publishResponse.EnsureSuccessStatusCode();
        var publishResult = await publishResponse.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        publishResult!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task UploadMedia_WithValidImage_ShouldUploadSuccessfully()
    {
        // Arrange
        var token = await AuthenticateAsync("uploadmedia@example.com", "uploadmedia_user");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Construct a valid PNG byte array
        var signature = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        var chunkLength = new byte[] { 0, 0, 0, 13 };
        var chunkType = new byte[] { 0x49, 0x48, 0x44, 0x52 };
        var widthBytes = BitConverter.GetBytes(100);
        if (BitConverter.IsLittleEndian) Array.Reverse(widthBytes);
        var heightBytes = BitConverter.GetBytes(100);
        if (BitConverter.IsLittleEndian) Array.Reverse(heightBytes);

        var ms = new MemoryStream();
        ms.Write(signature);
        ms.Write(chunkLength);
        ms.Write(chunkType);
        ms.Write(widthBytes);
        ms.Write(heightBytes);
        ms.Write(new byte[100]);
        var pngBytes = ms.ToArray();

        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(pngBytes);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/png");
        content.Add(fileContent, "file", "post_image.png");

        // Act
        var response = await _client.PostAsync("/api/v1/posts/media", content);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<MediaItemDto>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Type.Should().Be(MediaType.Image);
    }
}
