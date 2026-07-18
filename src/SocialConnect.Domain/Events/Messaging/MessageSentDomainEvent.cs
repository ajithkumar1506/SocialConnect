using SocialConnect.Domain.Common;

namespace SocialConnect.Domain.Events.Messaging;

public class MessageSentDomainEvent : IDomainEvent
{
    public Guid MessageId { get; }

    public MessageSentDomainEvent(Guid messageId)
    {
        MessageId = messageId;
    }
}
