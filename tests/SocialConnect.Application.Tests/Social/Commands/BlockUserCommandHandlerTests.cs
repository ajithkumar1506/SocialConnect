using FluentAssertions;
using Moq;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Features.Social.Commands.BlockUser;
using SocialConnect.Application.Tests.Users.Commands;
using SocialConnect.Domain.Entities.Social;
using SocialConnect.Domain.Entities.Users;
using SocialConnect.Domain.ValueObjects;

namespace SocialConnect.Application.Tests.Social.Commands;

public class BlockUserCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;

    public BlockUserCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldCreateBlockAndRemoveFollows()
    {
        // Arrange
        var blockerId = Guid.NewGuid();
        var blockedId = Guid.NewGuid();

        var blocker = User.Create(Email.Create("blocker@example.com"), "blocker", Password.Create("hash"));
        var blocked = User.Create(Email.Create("blocked@example.com"), "blocked", Password.Create("hash"));
        // Use reflection to set IDs
        typeof(User).GetProperty("Id")?.SetValue(blocker, blockerId);
        typeof(User).GetProperty("Id")?.SetValue(blocked, blockedId);

        var usersList = new List<User> { blocker, blocked };
        var usersDbSet = DbSetMockHelper.CreateMockDbSet(usersList);
        _contextMock.Setup(x => x.Users).Returns(usersDbSet);

        // Pre-existing follows
        var follow1 = Follow.Create(blockerId, blockedId);
        var follow2 = Follow.Create(blockedId, blockerId);
        var followsList = new List<Follow> { follow1, follow2 };
        var followsDbSet = DbSetMockHelper.CreateMockDbSet(followsList);
        _contextMock.Setup(x => x.Follows).Returns(followsDbSet);

        var blocksList = new List<Block>();
        var blocksDbSet = DbSetMockHelper.CreateMockDbSet(blocksList);
        _contextMock.Setup(x => x.Blocks).Returns(blocksDbSet);

        _currentUserServiceMock.Setup(x => x.UserId).Returns(blockerId);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new BlockUserCommandHandler(_contextMock.Object, _currentUserServiceMock.Object);
        var command = new BlockUserCommand(blockedId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _contextMock.Verify(x => x.Follows.RemoveRange(It.Is<IEnumerable<Follow>>(f => f.Count() == 2)), Times.Once);
        _contextMock.Verify(x => x.Blocks.AddAsync(It.Is<Block>(b => b.BlockerId == blockerId && b.BlockedId == blockedId), It.IsAny<CancellationToken>()), Times.Once);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_SelfBlock_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

        var handler = new BlockUserCommandHandler(_contextMock.Object, _currentUserServiceMock.Object);
        var command = new BlockUserCommand(userId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("You cannot block yourself.");
    }
}
