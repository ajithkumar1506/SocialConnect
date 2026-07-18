using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using SocialConnect.Application.Common.Interfaces;

namespace SocialConnect.Infrastructure.Services;

public class CacheService : ICacheService
{
    private readonly IDistributedCache _distributedCache;

    public CacheService(IDistributedCache distributedCache)
    {
        _distributedCache = distributedCache;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var cachedData = await _distributedCache.GetStringAsync(key, cancellationToken);

        if (cachedData == null)
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(cachedData);
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? slidingExpiration = null,
        CancellationToken cancellationToken = default
    )
    {
        var options = new DistributedCacheEntryOptions();

        if (slidingExpiration.HasValue)
        {
            options.SetSlidingExpiration(slidingExpiration.Value);
        }
        else
        {
            options.SetAbsoluteExpiration(TimeSpan.FromHours(1)); // Default absolute expiration
        }

        var serializedData = JsonSerializer.Serialize(value);
        await _distributedCache.SetStringAsync(key, serializedData, options, cancellationToken);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await _distributedCache.RemoveAsync(key, cancellationToken);
    }
}
