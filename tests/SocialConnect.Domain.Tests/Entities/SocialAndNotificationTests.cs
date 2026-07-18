using FluentAssertions;
using SocialConnect.Domain.Entities.Social;
using SocialConnect.Domain.Entities.Notifications;
using SocialConnect.Domain.Enums;

namespace SocialConnect.Domain.Tests.Entities;

public class SocialAndNotificationTests
{
    [Fact]
    public void CreateFollow_ShouldInitializeCorrectly()
    {
        // Arrange
        var followerId = Guid.NewGuid();
        var followingId = Guid.NewGuid();

        // Act
        var follow = Follow.Create(followerId, followingId);

        // Assert
        follow.FollowerId.Should().Be(followerId);
        follow.FollowingId.Should().Be(followingId);
        follow.CreatedAt.Should().BeBefore(DateTimeOffset.UtcNow.AddSeconds(1));
    }

    [Fact]
    public void CreateBlock_ShouldInitializeCorrectly()
    {
        // Arrange
        var blockerId = Guid.NewGuid();
        var blockedId = Guid.NewGuid();

        // Act
        var block = Block.Create(blockerId, blockedId);

        // Assert
        block.BlockerId.Should().Be(blockerId);
        block.BlockedId.Should().Be(blockedId);
        block.CreatedAt.Should().BeBefore(DateTimeOffset.UtcNow.AddSeconds(1));
    }

    [Fact]
    public void CreateMute_ShouldInitializeCorrectly()
    {
        // Arrange
        var muterId = Guid.NewGuid();
        var mutedId = Guid.NewGuid();

        // Act
        var mute = Mute.Create(muterId, mutedId);

        // Assert
        mute.MuterId.Should().Be(muterId);
        mute.MutedId.Should().Be(mutedId);
        mute.CreatedAt.Should().BeBefore(DateTimeOffset.UtcNow.AddSeconds(1));
    }

    [Fact]
    public void CreateNotification_ShouldInitializeCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var actorId = Guid.NewGuid();
        var targetId = Guid.NewGuid();

        // Act
        var notif = Notification.Create(userId, actorId, NotificationType.NewReaction, "User A liked your post", targetId, "Post");

        // Assert
        notif.UserId.Should().Be(userId);
        notif.ActorId.Should().Be(actorId);
        notif.Type.Should().Be(NotificationType.NewReaction);
        notif.Content.Should().Be("User A liked your post");
        notif.TargetId.Should().Be(targetId);
        notif.TargetType.Should().Be("Post");
        notif.IsRead.Should().BeFalse();
        notif.ReadAt.Should().BeNull();
        notif.DomainEvents.Should().HaveCount(1);
    }

    [Fact]
    public void MarkAsRead_ShouldUpdateStatusAndTimestamp()
    {
        // Arrange
        var notif = Notification.Create(Guid.NewGuid(), Guid.NewGuid(), NotificationType.NewFollower, "User A followed you");

        // Act
        notif.MarkAsRead();

        // Assert
        notif.IsRead.Should().BeTrue();
        notif.ReadAt.Should().NotBeNull();
    }
}
