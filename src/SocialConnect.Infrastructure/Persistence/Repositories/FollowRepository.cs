using Microsoft.EntityFrameworkCore;
using SocialConnect.Domain.Entities.Social;
using SocialConnect.Domain.Repositories;

namespace SocialConnect.Infrastructure.Persistence.Repositories;

public class FollowRepository : IFollowRepository
{
    private readonly ApplicationDbContext _dbContext;

    public FollowRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Follow?> GetFollowAsync(
        Guid followerId,
        Guid followingId,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbContext.Follows
            .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowingId == followingId, cancellationToken);
    }

    public async Task<IReadOnlyList<Follow>> GetFollowersAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbContext.Follows
            .Where(f => f.FollowingId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include("Follower.Profile")
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Follow>> GetFollowingAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbContext.Follows
            .Where(f => f.FollowerId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include("Following.Profile")
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetFollowersCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Follows
            .CountAsync(f => f.FollowingId == userId, cancellationToken);
    }

    public async Task<int> GetFollowingCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Follows
            .CountAsync(f => f.FollowerId == userId, cancellationToken);
    }

    public async Task AddAsync(Follow follow, CancellationToken cancellationToken = default)
    {
        await _dbContext.Follows.AddAsync(follow, cancellationToken);
    }

    public Task DeleteAsync(Follow follow, CancellationToken cancellationToken = default)
    {
        _dbContext.Follows.Remove(follow);
        return Task.CompletedTask;
    }
}
