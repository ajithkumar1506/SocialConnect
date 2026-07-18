using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;

namespace SocialConnect.API.Hubs;

[Authorize]
public class PresenceHub : Hub
{
    private readonly IDatabase _redisDb;
    private const string RedisHashKey = "online_users";

    public PresenceHub(IConnectionMultiplexer redis)
    {
        _redisDb = redis.GetDatabase();
    }

    public override async Task OnConnectedAsync()
    {
        var userIdStr = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(userIdStr, out var userId))
        {
            var userKey = userId.ToString();
            var newCount = await _redisDb.HashIncrementAsync(RedisHashKey, userKey, 1);

            if (newCount == 1)
            {
                await Clients.Others.SendAsync("UserOnline", userKey);
            }
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userIdStr = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(userIdStr, out var userId))
        {
            var userKey = userId.ToString();
            var newCount = await _redisDb.HashIncrementAsync(RedisHashKey, userKey, -1);

            if (newCount <= 0)
            {
                await _redisDb.HashDeleteAsync(RedisHashKey, userKey);
                await Clients.Others.SendAsync("UserOffline", userKey);
            }
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task<string[]> GetOnlineUsers()
    {
        var entries = await _redisDb.HashGetAllAsync(RedisHashKey);
        return entries
            .Where(e => (double)e.Value > 0)
            .Select(e => e.Name.ToString())
            .ToArray();
    }
}
