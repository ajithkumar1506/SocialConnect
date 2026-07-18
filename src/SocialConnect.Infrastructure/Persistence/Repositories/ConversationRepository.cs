using Microsoft.EntityFrameworkCore;
using SocialConnect.Domain.Entities.Messaging;
using SocialConnect.Domain.Repositories;

namespace SocialConnect.Infrastructure.Persistence.Repositories;

public class ConversationRepository : Repository<Conversation>, IConversationRepository
{
    public ConversationRepository(ApplicationDbContext dbContext)
        : base(dbContext) { }

    public async Task<Conversation?> GetByIdWithMembersAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        return await DbSet
            .Include(c => c.Members)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Conversation?> GetOneToOneConversationAsync(
        Guid user1Id,
        Guid user2Id,
        CancellationToken cancellationToken = default
    )
    {
        return await DbSet
            .Where(c => c.Type == Domain.Enums.ConversationType.OneToOne)
            .Where(c =>
                c.Members.Any(m => m.UserId == user1Id) && c.Members.Any(m => m.UserId == user2Id)
            )
            .Include(c => c.Members)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Conversation>> GetUserConversationsAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        return await DbSet
            .Where(c => c.Members.Any(m => m.UserId == userId))
            .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(c => c.Members)
            .ToListAsync(cancellationToken);
    }
}
