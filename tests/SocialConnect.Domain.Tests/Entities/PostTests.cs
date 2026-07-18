using FluentAssertions;
using SocialConnect.Domain.Entities.Posts;
using SocialConnect.Domain.Enums;
using SocialConnect.Domain.Events.Posts;
using SocialConnect.Domain.Exceptions;
using SocialConnect.Domain.ValueObjects;

namespace SocialConnect.Domain.Tests.Entities;

public class PostTests
{
    [Fact]
    public void Create_ShouldReturnDraftPost_AndRaisePostCreatedEvent()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var content = PostContent.Create("This is a test post");
        var type = PostType.Text;
        var status = PostStatus.Draft;

        // Act
        var post = Post.Create(authorId, content, type, status);

        // Assert
        post.AuthorId.Should().Be(authorId);
        post.Content.Should().Be(content);
        post.PostType.Should().Be(type);
        post.Status.Should().Be(status);

        post.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<PostCreatedDomainEvent>()
            .Which.PostId.Should().Be(post.Id);
    }

    [Fact]
    public void Publish_ShouldChangeStatusToPublished_AndRaisePostPublishedEvent()
    {
        // Arrange
        var post = Post.Create(Guid.NewGuid(), PostContent.Create("Draft post"), PostType.Text, PostStatus.Draft);
        post.ClearDomainEvents(); // Clear creation event

        // Act
        post.Publish();

        // Assert
        post.Status.Should().Be(PostStatus.Published);
        post.PublishedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));

        post.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<PostPublishedDomainEvent>()
            .Which.PostId.Should().Be(post.Id);
    }

    [Fact]
    public void UpdateContent_ShouldChangeContentProperty()
    {
        // Arrange
        var post = Post.Create(Guid.NewGuid(), PostContent.Create("Draft post"), PostType.Text, PostStatus.Draft);
        var newContent = PostContent.Create("Updated draft post");

        // Act
        post.UpdateContent(newContent);

        // Assert
        post.Content.Should().Be(newContent);
    }

    [Fact]
    public void Schedule_ShouldChangeStatusToScheduled_AndSetScheduleTime()
    {
        // Arrange
        var post = Post.Create(Guid.NewGuid(), PostContent.Create("Draft post"), PostType.Text, PostStatus.Draft);
        var scheduleTime = DateTimeOffset.UtcNow.AddHours(2);

        // Act
        post.Schedule(scheduleTime);

        // Assert
        post.Status.Should().Be(PostStatus.Scheduled);
        post.ScheduledAt.Should().Be(scheduleTime);
    }

    [Fact]
    public void Schedule_ForPublishedPost_ShouldThrowException()
    {
        // Arrange
        var post = Post.Create(Guid.NewGuid(), PostContent.Create("Draft post"), PostType.Text, PostStatus.Published);
        var scheduleTime = DateTimeOffset.UtcNow.AddHours(2);

        // Act
        Action action = () => post.Schedule(scheduleTime);

        // Assert
        action.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("Cannot schedule an already published post.");
    }

    [Fact]
    public void AddMedia_ShouldCreatePostMediaEntity()
    {
        // Arrange
        var post = Post.Create(Guid.NewGuid(), PostContent.Create("Draft post"), PostType.Text, PostStatus.Draft);
        var url = MediaUrl.Create("https://example.com/image.png");

        // Act
        post.AddMedia(url, MediaType.Image, 0, 1024, "https://example.com/thumb.png");

        // Assert
        post.Media.Should().HaveCount(1);
        var media = post.Media.First();
        media.MediaUrl.Should().Be(url);
        media.MediaType.Should().Be(MediaType.Image);
        media.OrderIndex.Should().Be(0);
        media.FileSize.Should().Be(1024);
        media.ThumbnailUrl.Should().Be("https://example.com/thumb.png");
    }

    [Fact]
    public void CommentAndReactionCounts_IncrementAndDecrement_ShouldWork()
    {
        // Arrange
        var post = Post.Create(Guid.NewGuid(), PostContent.Create("Draft post"), PostType.Text, PostStatus.Draft);

        // Act
        post.IncrementCommentCount();
        post.IncrementReactionCount();

        // Assert
        post.CommentCount.Should().Be(1);
        post.ReactionCount.Should().Be(1);

        // Act
        post.DecrementCommentCount();
        post.DecrementReactionCount();

        // Assert
        post.CommentCount.Should().Be(0);
        post.ReactionCount.Should().Be(0);
    }
}
