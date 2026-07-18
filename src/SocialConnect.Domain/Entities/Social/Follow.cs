using SocialConnect.Domain.Entities.Users;

namespace SocialConnect.Domain.Entities.Social;

public class Follow
{
    public Guid FollowerId { get; private set; }
    public Guid FollowingId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    // Navigation
    public User? Follower { get; private set; }
    public User? Following { get; private set; }

    private Follow() { }

    public static Follow Create(Guid followerId, Guid followingId)
    {
        return new Follow
        {
            FollowerId = followerId,
            FollowingId = followingId,
            CreatedAt = DateTimeOffset.UtcNow,
        };
    }
}
