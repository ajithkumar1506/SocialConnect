using Microsoft.EntityFrameworkCore;
using SocialConnect.Domain.Entities.Audit;
using SocialConnect.Domain.Entities.Messaging;
using SocialConnect.Domain.Entities.Notifications;
using SocialConnect.Domain.Entities.Posts;
using SocialConnect.Domain.Entities.Social;
using SocialConnect.Domain.Entities.Users;

namespace SocialConnect.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<UserProfile> UserProfiles { get; }
    DbSet<Role> Roles { get; }
    DbSet<RefreshToken> RefreshTokens { get; }

    DbSet<Post> Posts { get; }
    DbSet<PostMedia> PostMedia { get; }
    DbSet<Comment> Comments { get; }
    DbSet<Reaction> Reactions { get; }

    DbSet<Follow> Follows { get; }
    DbSet<Block> Blocks { get; }
    DbSet<Mute> Mutes { get; }

    DbSet<Conversation> Conversations { get; }
    DbSet<ConversationMember> ConversationMembers { get; }
    DbSet<Message> Messages { get; }
    DbSet<MessageAttachment> MessageAttachments { get; }

    DbSet<Notification> Notifications { get; }
    DbSet<AuditLog> AuditLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
