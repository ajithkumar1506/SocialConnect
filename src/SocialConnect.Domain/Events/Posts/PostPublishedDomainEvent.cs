using SocialConnect.Domain.Common;

namespace SocialConnect.Domain.Events.Posts;

public class PostPublishedDomainEvent : IDomainEvent
{
    public Guid PostId { get; }

    public PostPublishedDomainEvent(Guid postId)
    {
        PostId = postId;
    }
}
