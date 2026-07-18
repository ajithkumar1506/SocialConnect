using FluentAssertions;
using Moq;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Features.Messaging.Commands.DeleteMessage;
using SocialConnect.Domain.Entities.Messaging;
using SocialConnect.Domain.Enums;
using SocialConnect.Domain.Repositories;

namespace SocialConnect.Application.Tests.Messaging;

public class DeleteMessageCommandHandlerTests
{
    private readonly Mock<IMessageRepository> _messageRepositoryMock;
    private readonly Mock<IConversationRepository> _conversationRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;

    public DeleteMessageCommandHandlerTests()
    {
        _messageRepositoryMock = new Mock<IMessageRepository>();
        _conversationRepositoryMock = new Mock<IConversationRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
    }

    [Fact]
    public async Task Handle_SenderDeletingOwnMessage_ShouldSucceed()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var messageId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();

        var message = Message.Create(conversationId, currentUserId, "Delete me", MessageType.Text);

        _currentUserServiceMock.Setup(x => x.UserId).Returns(currentUserId);
        _messageRepositoryMock.Setup(x => x.GetByIdAsync(messageId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(message);

        var handler = new DeleteMessageCommandHandler(
            _messageRepositoryMock.Object,
            _conversationRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object
        );

        var command = new DeleteMessageCommand(messageId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        message.IsDeleted.Should().BeTrue();
        message.DeletedAt.Should().NotBeNull();
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_AdminDeletingOtherMessage_ShouldSucceed()
    {
        // Arrange
        var currentUserId = Guid.NewGuid(); // Admin
        var senderId = Guid.NewGuid();
        var messageId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();

        var message = Message.Create(conversationId, senderId, "Delete me", MessageType.Text);
        var conversation = Conversation.CreateGroup("Test Group", currentUserId); // Creator is Admin
        conversation.AddMember(senderId, ConversationMemberRole.Member);

        _currentUserServiceMock.Setup(x => x.UserId).Returns(currentUserId);
        _messageRepositoryMock.Setup(x => x.GetByIdAsync(messageId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(message);
        _conversationRepositoryMock.Setup(x => x.GetByIdWithMembersAsync(conversationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation);

        var handler = new DeleteMessageCommandHandler(
            _messageRepositoryMock.Object,
            _conversationRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object
        );

        var command = new DeleteMessageCommand(messageId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        message.IsDeleted.Should().BeTrue();
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NonAdminDeletingOtherMessage_ShouldFail()
    {
        // Arrange
        var currentUserId = Guid.NewGuid(); // Non-admin member
        var senderId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var messageId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();

        var message = Message.Create(conversationId, senderId, "Delete me", MessageType.Text);
        var conversation = Conversation.CreateGroup("Test Group", adminId);
        conversation.AddMember(currentUserId, ConversationMemberRole.Member);
        conversation.AddMember(senderId, ConversationMemberRole.Member);

        _currentUserServiceMock.Setup(x => x.UserId).Returns(currentUserId);
        _messageRepositoryMock.Setup(x => x.GetByIdAsync(messageId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(message);
        _conversationRepositoryMock.Setup(x => x.GetByIdWithMembersAsync(conversationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation);

        var handler = new DeleteMessageCommandHandler(
            _messageRepositoryMock.Object,
            _conversationRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object
        );

        var command = new DeleteMessageCommand(messageId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("You do not have permission to delete this message.");
        message.IsDeleted.Should().BeFalse();
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
