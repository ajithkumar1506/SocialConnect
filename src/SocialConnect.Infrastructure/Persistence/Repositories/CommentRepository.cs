using Microsoft.EntityFrameworkCore;
using SocialConnect.Domain.Entities.Posts;
using SocialConnect.Domain.Repositories;

namespace SocialConnect.Infrastructure.Persistence.Repositories;

public class CommentRepository : Repository<Comment>, ICommentRepository
{
    public CommentRepository(ApplicationDbContext dbContext)
        : base(dbContext) { }

    public async Task<IReadOnlyList<Comment>> GetByPostIdAsync(
        Guid postId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        return await DbSet
            .Where(c => c.PostId == postId && c.ParentCommentId == null)
            .OrderBy(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include("Author.Profile")
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Comment>> GetRepliesAsync(
        Guid commentId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        return await DbSet
            .Where(c => c.ParentCommentId == commentId)
            .OrderBy(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include("Author.Profile")
            .ToListAsync(cancellationToken);
    }
}
