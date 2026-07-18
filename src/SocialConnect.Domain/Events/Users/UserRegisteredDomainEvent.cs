using SocialConnect.Domain.Common;

namespace SocialConnect.Domain.Events.Users;

public class UserRegisteredDomainEvent : IDomainEvent
{
    public Guid UserId { get; }

    public UserRegisteredDomainEvent(Guid userId)
    {
        UserId = userId;
    }
}
