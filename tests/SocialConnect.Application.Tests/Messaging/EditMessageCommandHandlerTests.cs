using FluentAssertions;
using Moq;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Features.Messaging.Commands.EditMessage;
using SocialConnect.Domain.Entities.Messaging;
using SocialConnect.Domain.Enums;
using SocialConnect.Domain.Repositories;

namespace SocialConnect.Application.Tests.Messaging;

public class EditMessageCommandHandlerTests
{
    private readonly Mock<IMessageRepository> _messageRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;

    public EditMessageCommandHandlerTests()
    {
        _messageRepositoryMock = new Mock<IMessageRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldEditMessageContent()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var messageId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();

        var message = Message.Create(conversationId, currentUserId, "Original content", MessageType.Text);

        // Reflect messageId onto message since it was randomized
        typeof(Message).GetProperty("Id")?.SetValue(message, messageId);

        _currentUserServiceMock.Setup(x => x.UserId).Returns(currentUserId);
        _messageRepositoryMock.Setup(x => x.GetByIdAsync(messageId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(message);

        var handler = new EditMessageCommandHandler(
            _messageRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object
        );

        var command = new EditMessageCommand(messageId, "Updated content");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
        message.Content.Should().Be("Updated content");

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenMessageNotFound_ShouldReturnFailure()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var messageId = Guid.NewGuid();

        _currentUserServiceMock.Setup(x => x.UserId).Returns(currentUserId);
        _messageRepositoryMock.Setup(x => x.GetByIdAsync(messageId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Message?)null);

        var handler = new EditMessageCommandHandler(
            _messageRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object
        );

        var command = new EditMessageCommand(messageId, "Updated content");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Message not found.");
    }

    [Fact]
    public async Task Handle_WhenUserIsNotSender_ShouldReturnFailure()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var senderId = Guid.NewGuid();
        var messageId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();

        var message = Message.Create(conversationId, senderId, "Original content", MessageType.Text);

        _currentUserServiceMock.Setup(x => x.UserId).Returns(currentUserId);
        _messageRepositoryMock.Setup(x => x.GetByIdAsync(messageId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(message);

        var handler = new EditMessageCommandHandler(
            _messageRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object
        );

        var command = new EditMessageCommand(messageId, "Updated content");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("You can only edit your own messages.");
    }
}
