using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SocialConnect.API.Hubs;

[Authorize]
public class ChatHub : Hub
{
    public async Task JoinConversation(string conversationId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, conversationId);
    }

    public async Task LeaveConversation(string conversationId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId);
    }

    public async Task SendTypingStatus(string conversationId, bool isTyping)
    {
        var userIdStr = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdStr != null)
        {
            await Clients.Group(conversationId).SendAsync("UserTyping", userIdStr, isTyping);
        }
    }

    public async Task MarkMessageAsRead(string conversationId, string messageId)
    {
        var userIdStr = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdStr != null)
        {
            await Clients.Group(conversationId).SendAsync("MessageRead", messageId, userIdStr);
        }
    }
}
