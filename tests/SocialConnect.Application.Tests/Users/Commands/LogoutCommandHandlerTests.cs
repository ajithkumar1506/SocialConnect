using FluentAssertions;
using Moq;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Features.Auth.Commands.Logout;
using SocialConnect.Domain.Entities.Users;

namespace SocialConnect.Application.Tests.Users.Commands;

public class LogoutCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;

    public LogoutCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
    }

    [Fact]
    public async Task Handle_WithActiveToken_ShouldRevokeAndSave()
    {
        // Arrange
        var token = "active-refresh-token";
        var refreshToken = RefreshToken.Create(Guid.NewGuid(), token, "jwt-id", DateTimeOffset.UtcNow.AddDays(1), "127.0.0.1", "device");
        var tokensList = new List<RefreshToken> { refreshToken };
        var dbSet = DbSetMockHelper.CreateMockDbSet(tokensList);

        _contextMock.Setup(x => x.RefreshTokens).Returns(dbSet);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new LogoutCommandHandler(_contextMock.Object);

        // Act
        var result = await handler.Handle(new LogoutCommand(token), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        refreshToken.IsRevoked.Should().BeTrue();
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
