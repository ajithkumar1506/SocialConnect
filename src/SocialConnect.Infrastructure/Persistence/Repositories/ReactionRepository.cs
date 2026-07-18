using Microsoft.EntityFrameworkCore;
using SocialConnect.Domain.Entities.Posts;
using SocialConnect.Domain.Enums;
using SocialConnect.Domain.Repositories;

namespace SocialConnect.Infrastructure.Persistence.Repositories;

public class ReactionRepository : Repository<Reaction>, IReactionRepository
{
    public ReactionRepository(ApplicationDbContext dbContext)
        : base(dbContext) { }

    public async Task<Reaction?> GetUserReactionAsync(
        Guid userId,
        Guid targetId,
        TargetType targetType,
        CancellationToken cancellationToken = default
    )
    {
        return await DbSet
            .FirstOrDefaultAsync(
                r => r.UserId == userId && r.TargetId == targetId && r.TargetType == targetType,
                cancellationToken
            );
    }

    public async Task<IReadOnlyList<Reaction>> GetReactionsAsync(
        Guid targetId,
        TargetType targetType,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        return await DbSet
            .Where(r => r.TargetId == targetId && r.TargetType == targetType)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include("User.Profile")
            .ToListAsync(cancellationToken);
    }
}
