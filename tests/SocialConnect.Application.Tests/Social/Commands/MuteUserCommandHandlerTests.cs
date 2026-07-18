using FluentAssertions;
using Moq;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Features.Social.Commands.MuteUser;
using SocialConnect.Application.Tests.Users.Commands;
using SocialConnect.Domain.Entities.Social;
using SocialConnect.Domain.Entities.Users;
using SocialConnect.Domain.ValueObjects;

namespace SocialConnect.Application.Tests.Social.Commands;

public class MuteUserCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;

    public MuteUserCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldCreateMuteAndReturnSuccess()
    {
        // Arrange
        var muterId = Guid.NewGuid();
        var mutedId = Guid.NewGuid();

        var muter = User.Create(Email.Create("muter@example.com"), "muter", Password.Create("hash"));
        var muted = User.Create(Email.Create("muted@example.com"), "muted", Password.Create("hash"));
        // Use reflection to set IDs
        typeof(User).GetProperty("Id")?.SetValue(muter, muterId);
        typeof(User).GetProperty("Id")?.SetValue(muted, mutedId);

        var usersList = new List<User> { muter, muted };
        var usersDbSet = DbSetMockHelper.CreateMockDbSet(usersList);
        _contextMock.Setup(x => x.Users).Returns(usersDbSet);

        var mutesList = new List<Mute>();
        var mutesDbSet = DbSetMockHelper.CreateMockDbSet(mutesList);
        _contextMock.Setup(x => x.Mutes).Returns(mutesDbSet);

        _currentUserServiceMock.Setup(x => x.UserId).Returns(muterId);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new MuteUserCommandHandler(_contextMock.Object, _currentUserServiceMock.Object);
        var command = new MuteUserCommand(mutedId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _contextMock.Verify(x => x.Mutes.AddAsync(It.Is<Mute>(m => m.MuterId == muterId && m.MutedId == mutedId), It.IsAny<CancellationToken>()), Times.Once);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_SelfMute_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

        var handler = new MuteUserCommandHandler(_contextMock.Object, _currentUserServiceMock.Object);
        var command = new MuteUserCommand(userId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("You cannot mute yourself.");
    }
}
