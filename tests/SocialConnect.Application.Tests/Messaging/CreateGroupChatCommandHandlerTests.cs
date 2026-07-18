using FluentAssertions;
using Moq;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Features.Messaging.Commands.CreateGroupChat;
using SocialConnect.Domain.Entities.Messaging;
using SocialConnect.Domain.Entities.Users;
using SocialConnect.Domain.Repositories;
using SocialConnect.Domain.ValueObjects;

namespace SocialConnect.Application.Tests.Messaging;

public class CreateGroupChatCommandHandlerTests
{
    private readonly Mock<IConversationRepository> _conversationRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;

    public CreateGroupChatCommandHandlerTests()
    {
        _conversationRepositoryMock = new Mock<IConversationRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldCreateGroupAndAddMembers()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var memberId1 = Guid.NewGuid();
        var memberId2 = Guid.NewGuid();

        var user1 = User.Create(Email.Create("user1@example.com"), "user1", Password.Create("hash"));
        var user2 = User.Create(Email.Create("user2@example.com"), "user2", Password.Create("hash"));

        _currentUserServiceMock.Setup(x => x.UserId).Returns(currentUserId);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(memberId1, It.IsAny<CancellationToken>())).ReturnsAsync(user1);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(memberId2, It.IsAny<CancellationToken>())).ReturnsAsync(user2);

        var handler = new CreateGroupChatCommandHandler(
            _conversationRepositoryMock.Object,
            _userRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object
        );

        var command = new CreateGroupChatCommand("Test Group", new List<Guid> { memberId1, memberId2 }, "http://example.com/image.png");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        _conversationRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Conversation>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserNotLoggedIn_ShouldReturnFailure()
    {
        // Arrange
        _currentUserServiceMock.Setup(x => x.UserId).Returns((Guid?)null);

        var handler = new CreateGroupChatCommandHandler(
            _conversationRepositoryMock.Object,
            _userRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object
        );

        var command = new CreateGroupChatCommand("Test Group", new List<Guid> { Guid.NewGuid() });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("User must be logged in.");
        _conversationRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Conversation>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
