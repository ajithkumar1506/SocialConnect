using SocialConnect.Domain.Entities.Users;

namespace SocialConnect.Domain.Entities.Social;

public class Mute
{
    public Guid MuterId { get; private set; }
    public Guid MutedId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    // Navigation
    public User? Muter { get; private set; }
    public User? Muted { get; private set; }

    private Mute() { }

    public static Mute Create(Guid muterId, Guid mutedId)
    {
        return new Mute
        {
            MuterId = muterId,
            MutedId = mutedId,
            CreatedAt = DateTimeOffset.UtcNow,
        };
    }
}
