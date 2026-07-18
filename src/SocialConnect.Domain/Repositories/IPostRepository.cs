using SocialConnect.Domain.Entities.Posts;

namespace SocialConnect.Domain.Repositories;

public interface IPostRepository : IRepository<Post>
{
    Task<Post?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Post>> GetFeedAsync(
        Guid userId,
        int pageSize,
        DateTimeOffset? cursorScore,
        Guid? cursorId,
        CancellationToken cancellationToken = default
    );
    Task<IReadOnlyList<Post>> GetUserPostsAsync(
        Guid userId,
        int pageSize,
        DateTimeOffset? cursorScore,
        Guid? cursorId,
        CancellationToken cancellationToken = default
    );
}
