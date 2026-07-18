using SocialConnect.Domain.Entities.Messaging;

namespace SocialConnect.Domain.Repositories;

public interface IMessageRepository : IRepository<Message>
{
    Task<IReadOnlyList<Message>> GetConversationMessagesAsync(
        Guid conversationId,
        int pageSize,
        DateTimeOffset? before,
        CancellationToken cancellationToken = default
    );
}
