using SocialConnect.Domain.Common;
using SocialConnect.Domain.Entities.Users;
using SocialConnect.Domain.Enums;

namespace SocialConnect.Domain.Entities.Posts;

public class Reaction : Entity
{
    public Guid UserId { get; private set; }
    public Guid TargetId { get; private set; }
    public TargetType TargetType { get; private set; }
    public ReactionType ReactionType { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    // Navigation
    public User? User { get; private set; }

    private Reaction() { }

    public static Reaction Create(
        Guid userId,
        Guid targetId,
        TargetType targetType,
        ReactionType reactionType
    )
    {
        return new Reaction
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TargetId = targetId,
            TargetType = targetType,
            ReactionType = reactionType,
            CreatedAt = DateTimeOffset.UtcNow,
        };
    }
}
