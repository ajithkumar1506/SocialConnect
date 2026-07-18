using SocialConnect.Domain.Entities.Users;

namespace SocialConnect.Domain.Entities.Social;

public class Block
{
    public Guid BlockerId { get; private set; }
    public Guid BlockedId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    // Navigation
    public User? Blocker { get; private set; }
    public User? Blocked { get; private set; }

    private Block() { }

    public static Block Create(Guid blockerId, Guid blockedId)
    {
        return new Block
        {
            BlockerId = blockerId,
            BlockedId = blockedId,
            CreatedAt = DateTimeOffset.UtcNow,
        };
    }
}
