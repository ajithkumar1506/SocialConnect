using SocialConnect.Domain.Common;

namespace SocialConnect.Domain.Events.Users;

public class UserVerifiedDomainEvent : IDomainEvent
{
    public Guid UserId { get; }

    public UserVerifiedDomainEvent(Guid userId)
    {
        UserId = userId;
    }
}
