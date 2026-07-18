using SocialConnect.Domain.Common;

namespace SocialConnect.Domain.Events.Posts;

public class PostCreatedDomainEvent : IDomainEvent
{
    public Guid PostId { get; }

    public PostCreatedDomainEvent(Guid postId)
    {
        PostId = postId;
    }
}
