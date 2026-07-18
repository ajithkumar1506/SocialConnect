using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SocialConnect.Application.Common.Models;
using SocialConnect.Application.Features.Auth.Commands.Register;
using SocialConnect.Application.Features.Messaging.Commands.CreateConversation;
using SocialConnect.Application.Features.Messaging.Commands.CreateGroupChat;
using SocialConnect.Application.Features.Messaging.Commands.SendMessage;
using SocialConnect.Application.Features.Messaging.DTOs;
using SocialConnect.Domain.Enums;
using SocialConnect.Infrastructure.Persistence;

namespace SocialConnect.API.Tests;

public class MessagingIntegrationTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly HttpClient _client;
    private readonly IntegrationTestWebAppFactory _factory;

    public MessagingIntegrationTests(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();

        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
    }

    private async Task<(string Token, Guid UserId)> RegisterAndAuthenticateUserAsync(string email, string username)
    {
        // 1. Register User
        var registerCommand = new RegisterCommand(email, "Password123!", username, "First", "Last");
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", registerCommand);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResult>>();
        var token = result!.Data!.Token;

        // 2. Fetch User Guid from DB
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await context.Users.FirstAsync(u => u.UserName == username);

        return (token, user.Id);
    }

    [Fact]
    public async Task MessagingFullLifecycle_ShouldSucceed()
    {
        // Arrange: Register User A and User B
        var (tokenA, userIdA) = await RegisterAndAuthenticateUserAsync("usera@example.com", "usera_chat");
        var (tokenB, userIdB) = await RegisterAndAuthenticateUserAsync("userb@example.com", "userb_chat");

        // --- 1. User A creates a 1-to-1 conversation with User B ---
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
        var createConvCmd = new CreateConversationCommand(userIdB);
        var createConvResponse = await _client.PostAsJsonAsync("/api/v1/messages/conversation", createConvCmd);
        createConvResponse.EnsureSuccessStatusCode();

        var createConvResult = await createConvResponse.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
        var conversationId = createConvResult!.Data;
        conversationId.Should().NotBeEmpty();

        // --- 2. User A sends a message in the conversation ---
        var sendMessageCmd = new SendMessageCommand(conversationId, "Hello from User A!", MessageType.Text);
        var sendMsgResponse = await _client.PostAsJsonAsync("/api/v1/messages", sendMessageCmd);
        sendMsgResponse.EnsureSuccessStatusCode();

        var sendMsgResult = await sendMsgResponse.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
        var messageId = sendMsgResult!.Data;
        messageId.Should().NotBeEmpty();

        // --- 3. User B retrieves their conversations and verify last message and unread count ---
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenB);
        var getConvsResponse = await _client.GetAsync("/api/v1/messages/conversations?pageNumber=1&pageSize=10");
        getConvsResponse.EnsureSuccessStatusCode();

        var getConvsResult = await getConvsResponse.Content.ReadFromJsonAsync<ApiResponse<PaginatedList<ConversationDto>>>();
        var conversations = getConvsResult!.Data!.Items;
        conversations.Should().Contain(c => c.Id == conversationId);

        var conversationDto = conversations.First(c => c.Id == conversationId);
        conversationDto.UnreadCount.Should().Be(1);
        conversationDto.LastMessage.Should().NotBeNull();
        conversationDto.LastMessage!.Content.Should().Be("Hello from User A!");

        // --- 4. User B marks conversation as read and checks unread count again ---
        var readResponse = await _client.PostAsync($"/api/v1/messages/conversation/{conversationId}/read", null);
        readResponse.EnsureSuccessStatusCode();

        var getConvsResponse2 = await _client.GetAsync("/api/v1/messages/conversations?pageNumber=1&pageSize=10");
        getConvsResponse2.EnsureSuccessStatusCode();
        var getConvsResult2 = await getConvsResponse2.Content.ReadFromJsonAsync<ApiResponse<PaginatedList<ConversationDto>>>();
        var conversationDto2 = getConvsResult2!.Data!.Items.First(c => c.Id == conversationId);
        conversationDto2.UnreadCount.Should().Be(0);

        // --- 5. User A edits the message ---
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
        var editResponse = await _client.PutAsJsonAsync($"/api/v1/messages/{messageId}", "Edited content by A!");
        editResponse.EnsureSuccessStatusCode();

        // Verify edited content via messages query
        var getMsgsResponse = await _client.GetAsync($"/api/v1/messages/conversation/{conversationId}?pageNumber=1&pageSize=20");
        getMsgsResponse.EnsureSuccessStatusCode();
        var getMsgsResult = await getMsgsResponse.Content.ReadFromJsonAsync<ApiResponse<PaginatedList<MessageDto>>>();
        getMsgsResult!.Data!.Items.Should().Contain(m => m.Id == messageId && m.Content == "Edited content by A!");

        // --- 6. User A deletes the message ---
        var deleteResponse = await _client.DeleteAsync($"/api/v1/messages/{messageId}");
        deleteResponse.EnsureSuccessStatusCode();

        // Verify message is deleted from the messages list
        var getMsgsResponse2 = await _client.GetAsync($"/api/v1/messages/conversation/{conversationId}?pageNumber=1&pageSize=20");
        getMsgsResponse2.EnsureSuccessStatusCode();
        var getMsgsResult2 = await getMsgsResponse2.Content.ReadFromJsonAsync<ApiResponse<PaginatedList<MessageDto>>>();
        getMsgsResult2!.Data!.Items.Should().NotContain(m => m.Id == messageId);
    }

    [Fact]
    public async Task GroupChatLifecycle_ShouldSucceed()
    {
        // Arrange: Register User A, User B, User C
        var (tokenA, userIdA) = await RegisterAndAuthenticateUserAsync("user_admin@example.com", "user_admin");
        var (tokenB, userIdB) = await RegisterAndAuthenticateUserAsync("user_member1@example.com", "user_member1");
        var (tokenC, userIdC) = await RegisterAndAuthenticateUserAsync("user_member2@example.com", "user_member2");

        // --- 1. User A creates a group chat with User B ---
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
        var createGroupCmd = new CreateGroupChatCommand("Best Friends Group", new List<Guid> { userIdB });
        var createResponse = await _client.PostAsJsonAsync("/api/v1/messages/group", createGroupCmd);
        createResponse.EnsureSuccessStatusCode();

        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
        var groupId = createResult!.Data;
        groupId.Should().NotBeEmpty();

        // --- 2. User A adds User C to the group chat ---
        var addMemberResponse = await _client.PostAsJsonAsync($"/api/v1/messages/conversation/{groupId}/members", userIdC);
        addMemberResponse.EnsureSuccessStatusCode();

        // --- 3. Verify User C is in the conversation members ---
        var getConvsResponse = await _client.GetAsync("/api/v1/messages/conversations?pageNumber=1&pageSize=10");
        getConvsResponse.EnsureSuccessStatusCode();
        var getConvsResult = await getConvsResponse.Content.ReadFromJsonAsync<ApiResponse<PaginatedList<ConversationDto>>>();
        var groupDto = getConvsResult!.Data!.Items.First(c => c.Id == groupId);
        groupDto.Members.Should().Contain(m => m.UserId == userIdC);

        // --- 4. User A removes User B from the group chat ---
        var removeMemberResponse = await _client.DeleteAsync($"/api/v1/messages/conversation/{groupId}/members/{userIdB}");
        removeMemberResponse.EnsureSuccessStatusCode();

        // Verify User B is no longer in the conversation members
        var getConvsResponse2 = await _client.GetAsync("/api/v1/messages/conversations?pageNumber=1&pageSize=10");
        getConvsResponse2.EnsureSuccessStatusCode();
        var getConvsResult2 = await getConvsResponse2.Content.ReadFromJsonAsync<ApiResponse<PaginatedList<ConversationDto>>>();
        var groupDto2 = getConvsResult2!.Data!.Items.First(c => c.Id == groupId);
        groupDto2.Members.Should().NotContain(m => m.UserId == userIdB);
    }
}
