using Microsoft.EntityFrameworkCore;
using SocialConnect.Domain.Entities.Posts;
using SocialConnect.Domain.Repositories;

namespace SocialConnect.Infrastructure.Persistence.Repositories;

public class PostRepository : Repository<Post>, IPostRepository
{
    public PostRepository(ApplicationDbContext dbContext)
        : base(dbContext) { }

    public async Task<Post?> GetByIdWithDetailsAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        return await DbSet
            .Include(p => p.Author)
            .Include(p => p.Media)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Post>> GetFeedAsync(
        Guid userId,
        int pageSize,
        DateTimeOffset? cursorScore,
        Guid? cursorId,
        CancellationToken cancellationToken = default
    )
    {
        var query = DbContext
            .Posts.Where(p =>
                DbContext.Follows.Any(f => f.FollowerId == userId && f.FollowingId == p.AuthorId)
            )
            .Where(p => p.Status == Domain.Enums.PostStatus.Published);

        if (cursorScore.HasValue && cursorId.HasValue)
        {
            query = query.Where(p =>
                p.PublishedAt < cursorScore
                || (p.PublishedAt == cursorScore && p.Id.CompareTo(cursorId.Value) < 0)
            );
        }

        return await query
            .OrderByDescending(p => p.PublishedAt)
            .ThenByDescending(p => p.Id)
            .Take(pageSize)
            .Include(p => p.Author)
            .Include(p => p.Media)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Post>> GetUserPostsAsync(
        Guid userId,
        int pageSize,
        DateTimeOffset? cursorScore,
        Guid? cursorId,
        CancellationToken cancellationToken = default
    )
    {
        var query = DbSet.Where(p =>
            p.AuthorId == userId && p.Status == Domain.Enums.PostStatus.Published
        );

        if (cursorScore.HasValue && cursorId.HasValue)
        {
            query = query.Where(p =>
                p.PublishedAt < cursorScore
                || (p.PublishedAt == cursorScore && p.Id.CompareTo(cursorId.Value) < 0)
            );
        }

        return await query
            .OrderByDescending(p => p.PublishedAt)
            .ThenByDescending(p => p.Id)
            .Take(pageSize)
            .Include(p => p.Media)
            .ToListAsync(cancellationToken);
    }
}
