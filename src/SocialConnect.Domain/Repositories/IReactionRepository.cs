using SocialConnect.Domain.Entities.Posts;
using SocialConnect.Domain.Enums;

namespace SocialConnect.Domain.Repositories;

public interface IReactionRepository : IRepository<Reaction>
{
    Task<Reaction?> GetUserReactionAsync(
        Guid userId,
        Guid targetId,
        TargetType targetType,
        CancellationToken cancellationToken = default
    );
    Task<IReadOnlyList<Reaction>> GetReactionsAsync(
        Guid targetId,
        TargetType targetType,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    );
}
