using SocialConnect.Domain.Entities.Users;
using SocialConnect.Domain.Enums;

namespace SocialConnect.Domain.Entities.Messaging;

public class ConversationMember
{
    public Guid ConversationId { get; private set; }
    public Guid UserId { get; private set; }
    public ConversationMemberRole Role { get; private set; }
    public bool IsMuted { get; private set; }
    public DateTimeOffset JoinedAt { get; private set; }
    public DateTimeOffset? LastReadAt { get; private set; }

    // Navigation
    public Conversation? Conversation { get; private set; }
    public User? User { get; private set; }

    private ConversationMember() { }

    internal static ConversationMember Create(
        Guid conversationId,
        Guid userId,
        ConversationMemberRole role
    )
    {
        return new ConversationMember
        {
            ConversationId = conversationId,
            UserId = userId,
            Role = role,
            IsMuted = false,
            JoinedAt = DateTimeOffset.UtcNow,
        };
    }

    public void ChangeRole(ConversationMemberRole role)
    {
        Role = role;
    }

    public void Mute()
    {
        IsMuted = true;
    }

    public void Unmute()
    {
        IsMuted = false;
    }

    public void UpdateLastReadAt(DateTimeOffset timestamp)
    {
        LastReadAt = timestamp;
    }
}
