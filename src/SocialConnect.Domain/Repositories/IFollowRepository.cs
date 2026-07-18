using SocialConnect.Domain.Entities.Social;

namespace SocialConnect.Domain.Repositories;

public interface IFollowRepository
{
    Task<Follow?> GetFollowAsync(
        Guid followerId,
        Guid followingId,
        CancellationToken cancellationToken = default
    );
    Task<IReadOnlyList<Follow>> GetFollowersAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    );
    Task<IReadOnlyList<Follow>> GetFollowingAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    );
    Task<int> GetFollowersCountAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<int> GetFollowingCountAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(Follow follow, CancellationToken cancellationToken = default);
    Task DeleteAsync(Follow follow, CancellationToken cancellationToken = default);
}
