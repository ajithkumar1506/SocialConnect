using SocialConnect.Domain.Entities.Posts;

namespace SocialConnect.Domain.Repositories;

public interface ICommentRepository : IRepository<Comment>
{
    Task<IReadOnlyList<Comment>> GetByPostIdAsync(
        Guid postId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    );
    Task<IReadOnlyList<Comment>> GetRepliesAsync(
        Guid commentId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    );
}
