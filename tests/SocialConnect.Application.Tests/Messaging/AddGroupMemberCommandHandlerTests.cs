using FluentAssertions;
using Moq;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Features.Messaging.Commands.AddGroupMember;
using SocialConnect.Domain.Entities.Messaging;
using SocialConnect.Domain.Entities.Users;
using SocialConnect.Domain.Repositories;
using SocialConnect.Domain.ValueObjects;

namespace SocialConnect.Application.Tests.Messaging;

public class AddGroupMemberCommandHandlerTests
{
    private readonly Mock<IConversationRepository> _conversationRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;

    public AddGroupMemberCommandHandlerTests()
    {
        _conversationRepositoryMock = new Mock<IConversationRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldAddMemberAndSaveChanges()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        var userToAddId = Guid.NewGuid();

        var conversation = Conversation.CreateGroup("Test Group", currentUserId);
        var userToAdd = User.Create(Email.Create("user@example.com"), "user", Password.Create("hash"));

        _currentUserServiceMock.Setup(x => x.UserId).Returns(currentUserId);
        _conversationRepositoryMock.Setup(x => x.GetByIdWithMembersAsync(conversationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userToAddId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userToAdd);

        var handler = new AddGroupMemberCommandHandler(
            _conversationRepositoryMock.Object,
            _userRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object
        );

        var command = new AddGroupMemberCommand(conversationId, userToAddId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
        conversation.Members.Should().Contain(m => m.UserId == userToAddId);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenConversationNotFound_ShouldReturnFailure()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();

        _currentUserServiceMock.Setup(x => x.UserId).Returns(currentUserId);
        _conversationRepositoryMock.Setup(x => x.GetByIdWithMembersAsync(conversationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Conversation?)null);

        var handler = new AddGroupMemberCommandHandler(
            _conversationRepositoryMock.Object,
            _userRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object
        );

        var command = new AddGroupMemberCommand(conversationId, Guid.NewGuid());

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Conversation not found.");
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenConversationIsOneToOne_ShouldReturnFailure()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();

        var conversation = Conversation.CreateOneToOne(currentUserId, otherUserId);

        _currentUserServiceMock.Setup(x => x.UserId).Returns(currentUserId);
        _conversationRepositoryMock.Setup(x => x.GetByIdWithMembersAsync(conversationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation);

        var handler = new AddGroupMemberCommandHandler(
            _conversationRepositoryMock.Object,
            _userRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object
        );

        var command = new AddGroupMemberCommand(conversationId, Guid.NewGuid());

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Cannot add members to a one-to-one conversation.");
    }

    [Fact]
    public async Task Handle_WhenCurrentUserIsNotMember_ShouldReturnFailure()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var creatorId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();

        var conversation = Conversation.CreateGroup("Test Group", creatorId);

        _currentUserServiceMock.Setup(x => x.UserId).Returns(currentUserId);
        _conversationRepositoryMock.Setup(x => x.GetByIdWithMembersAsync(conversationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation);

        var handler = new AddGroupMemberCommandHandler(
            _conversationRepositoryMock.Object,
            _userRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object
        );

        var command = new AddGroupMemberCommand(conversationId, Guid.NewGuid());

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("You must be a member of the group to add others.");
    }

    [Fact]
    public async Task Handle_WhenUserToAddNotFound_ShouldReturnFailure()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        var userToAddId = Guid.NewGuid();

        var conversation = Conversation.CreateGroup("Test Group", currentUserId);

        _currentUserServiceMock.Setup(x => x.UserId).Returns(currentUserId);
        _conversationRepositoryMock.Setup(x => x.GetByIdWithMembersAsync(conversationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userToAddId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var handler = new AddGroupMemberCommandHandler(
            _conversationRepositoryMock.Object,
            _userRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object
        );

        var command = new AddGroupMemberCommand(conversationId, userToAddId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("User to add not found.");
    }
}
