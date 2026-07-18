using SocialConnect.Domain.Entities.Messaging;

namespace SocialConnect.Domain.Repositories;

public interface IConversationRepository : IRepository<Conversation>
{
    Task<Conversation?> GetOneToOneConversationAsync(
        Guid user1Id,
        Guid user2Id,
        CancellationToken cancellationToken = default
    );
    Task<IReadOnlyList<Conversation>> GetUserConversationsAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    );
    Task<Conversation?> GetByIdWithMembersAsync(
        Guid id,
        CancellationToken cancellationToken = default
    );
}
