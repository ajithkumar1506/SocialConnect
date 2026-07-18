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
using SocialConnect.Application.Features.Users.Commands.UpdateProfile;
using SocialConnect.Application.Features.Users.DTOs;
using SocialConnect.Infrastructure.Persistence;

namespace SocialConnect.API.Tests;

public class UsersIntegrationTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;

    public UsersIntegrationTests(IntegrationTestWebAppFactory factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                // Replace IFileStorageService with a fake implementation for integration tests
                var fileStorageMock = new Mock<IFileStorageService>();
                fileStorageMock
                    .Setup(x => x.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Stream stream, string name, string contentType, CancellationToken ct) => $"http://fake-storage.local/{name}");

                services.RemoveAll<IFileStorageService>();
                services.AddSingleton<IFileStorageService>(fileStorageMock.Object);
            });
        });
        _client = _factory.CreateClient();

        // Ensure database is created and migrations are applied
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
            // Try to login if registration failed (already exists)
            var loginCommand = new LoginCommand(email, "Password123!");
            var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", loginCommand);
            var res = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<AuthResult>>();
            authResult = res?.Data;
        }

        return authResult?.Token ?? string.Empty;
    }

    [Fact]
    public async Task UpdateProfile_WithValidData_ShouldUpdateAndReturnUserProfile()
    {
        // Arrange
        var token = await AuthenticateAsync("updateprofile@example.com", "updateprofile_user");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var updateCommand = new UpdateProfileCommand(
            "UpdatedFirst",
            "UpdatedLast",
            "Updated Headline",
            "Updated Bio",
            "Updated Location",
            new DateOnly(1995, 5, 5)
        );

        // Act
        var response = await _client.PutAsJsonAsync("/api/v1/users/me", updateCommand);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<UserProfileDto>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.FirstName.Should().Be("UpdatedFirst");
        result.Data.LastName.Should().Be("UpdatedLast");
        result.Data.Headline.Should().Be("Updated Headline");
        result.Data.Bio.Should().Be("Updated Bio");
        result.Data.Location.Should().Be("Updated Location");
        result.Data.DateOfBirth.Should().Be(new DateOnly(1995, 5, 5));
    }

    [Fact]
    public async Task UploadProfileImage_WithValidFile_ShouldUploadAndReturnUrl()
    {
        // Arrange
        var token = await AuthenticateAsync("uploadprofileimg@example.com", "uploadprofileimg_user");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[] { 1, 2, 3, 4 });
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/png");
        content.Add(fileContent, "file", "avatar.png");

        // Act
        var response = await _client.PostAsync("/api/v1/users/me/profile-image", content);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<string>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().Contain("avatar.png");
    }

    [Fact]
    public async Task UploadCoverImage_WithValidFile_ShouldUploadAndReturnUrl()
    {
        // Arrange
        var token = await AuthenticateAsync("uploadcoverimg@example.com", "uploadcoverimg_user");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[] { 1, 2, 3, 4 });
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
        content.Add(fileContent, "file", "cover.jpg");

        // Act
        var response = await _client.PostAsync("/api/v1/users/me/cover-image", content);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<string>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().Contain("cover.jpg");
    }

    [Fact]
    public async Task SearchUsers_WithSearchQuery_ShouldReturnMatchingProfiles()
    {
        // Arrange
        var token = await AuthenticateAsync("searchusers@example.com", "searchusers_user");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/v1/users/search?searchTerm=searchusers");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<PaginatedList<UserProfileDto>>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetUserById_WithValidId_ShouldReturnUserDto()
    {
        // Arrange
        var token = await AuthenticateAsync("getuserbyid@example.com", "getuserbyid_user");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Get current user profile first to obtain UserId
        var meResponse = await _client.GetAsync("/api/v1/users/me");
        var meResult = await meResponse.Content.ReadFromJsonAsync<ApiResponse<UserProfileDto>>();
        var userId = meResult!.Data!.UserId;

        // Act
        var response = await _client.GetAsync($"/api/v1/users/{userId}/details");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<UserDto>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(userId);
        result.Data.UserName.Should().Be("getuserbyid_user");
        result.Data.Email.Should().Be("getuserbyid@example.com");
        result.Data.Profile.Should().NotBeNull();
    }
}
