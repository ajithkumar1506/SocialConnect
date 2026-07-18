using Microsoft.EntityFrameworkCore;
using SocialConnect.Domain.Entities.Messaging;
using SocialConnect.Domain.Repositories;

namespace SocialConnect.Infrastructure.Persistence.Repositories;

public class MessageRepository : Repository<Message>, IMessageRepository
{
    public MessageRepository(ApplicationDbContext dbContext)
        : base(dbContext) { }

    public async Task<IReadOnlyList<Message>> GetConversationMessagesAsync(
        Guid conversationId,
        int pageSize,
        DateTimeOffset? before,
        CancellationToken cancellationToken = default
    )
    {
        var query = DbSet.Where(m => m.ConversationId == conversationId);

        if (before.HasValue)
        {
            query = query.Where(m => m.CreatedAt < before.Value);
        }

        return await query
            .OrderByDescending(m => m.CreatedAt)
            .Take(pageSize)
            .Include(m => m.Attachments)
            .ToListAsync(cancellationToken);
    }
}
