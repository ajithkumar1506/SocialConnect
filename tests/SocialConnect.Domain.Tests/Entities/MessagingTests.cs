using FluentAssertions;
using SocialConnect.Domain.Entities.Messaging;
using SocialConnect.Domain.Enums;
using SocialConnect.Domain.Exceptions;

namespace SocialConnect.Domain.Tests.Entities;

public class MessagingTests
{
    [Fact]
    public void CreateOneToOne_ShouldInitializeCorrectly()
    {
        // Arrange
        var user1Id = Guid.NewGuid();
        var user2Id = Guid.NewGuid();

        // Act
        var conv = Conversation.CreateOneToOne(user1Id, user2Id);

        // Assert
        conv.Type.Should().Be(ConversationType.OneToOne);
        conv.Members.Should().HaveCount(2);
        conv.Members.Should().Contain(m => m.UserId == user1Id && m.Role == ConversationMemberRole.Member);
        conv.Members.Should().Contain(m => m.UserId == user2Id && m.Role == ConversationMemberRole.Member);
    }

    [Fact]
    public void CreateGroup_ShouldInitializeCorrectly()
    {
        // Arrange
        var creatorId = Guid.NewGuid();

        // Act
        var conv = Conversation.CreateGroup("Work Group", creatorId, "http://image.png");

        // Assert
        conv.Type.Should().Be(ConversationType.Group);
        conv.Name.Should().Be("Work Group");
        conv.ImageUrl.Should().Be("http://image.png");
        conv.Members.Should().HaveCount(1);
        conv.Members.First().UserId.Should().Be(creatorId);
        conv.Members.First().Role.Should().Be(ConversationMemberRole.Admin);
    }

    [Fact]
    public void CreateGroup_WithEmptyName_ShouldThrowException()
    {
        // Act
        Action action = () => Conversation.CreateGroup("", Guid.NewGuid());

        // Assert
        action.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("Group conversation must have a name.");
    }

    [Fact]
    public void UpdateGroupDetails_ForOneToOne_ShouldThrowException()
    {
        // Arrange
        var conv = Conversation.CreateOneToOne(Guid.NewGuid(), Guid.NewGuid());

        // Act
        Action action = () => conv.UpdateGroupDetails("New Name", null);

        // Assert
        action.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("Can only update details for a group conversation.");
    }

    [Fact]
    public void CreateMessage_ShouldInitializeCorrectly()
    {
        // Arrange
        var convId = Guid.NewGuid();
        var senderId = Guid.NewGuid();

        // Act
        var msg = Message.Create(convId, senderId, "Hello", MessageType.Text);

        // Assert
        msg.ConversationId.Should().Be(convId);
        msg.SenderId.Should().Be(senderId);
        msg.Content.Should().Be("Hello");
        msg.MessageType.Should().Be(MessageType.Text);
        msg.Status.Should().Be(MessageStatus.Sent);
    }

    [Fact]
    public void EditMessage_ShouldUpdateContentAndTimestamp()
    {
        // Arrange
        var msg = Message.Create(Guid.NewGuid(), Guid.NewGuid(), "Hello", MessageType.Text);

        // Act
        msg.Edit("Hello edited");

        // Assert
        msg.Content.Should().Be("Hello edited");
        msg.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void AddAttachment_ShouldCreateAttachmentEntity()
    {
        // Arrange
        var msg = Message.Create(Guid.NewGuid(), Guid.NewGuid(), "Hello", MessageType.Text);

        // Act
        msg.AddAttachment("http://file.png", "file.png", 1024, "image/png");

        // Assert
        msg.Attachments.Should().HaveCount(1);
        var att = msg.Attachments.First();
        att.FileUrl.Should().Be("http://file.png");
        att.FileName.Should().Be("file.png");
        att.FileSize.Should().Be(1024);
        att.ContentType.Should().Be("image/png");
    }
}
