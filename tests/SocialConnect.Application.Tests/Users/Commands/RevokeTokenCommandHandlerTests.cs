using FluentAssertions;
using Moq;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Features.Auth.Commands.RevokeToken;
using SocialConnect.Domain.Entities.Users;

namespace SocialConnect.Application.Tests.Users.Commands;

public class RevokeTokenCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;

    public RevokeTokenCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
    }

    [Fact]
    public async Task Handle_WithActiveToken_ShouldRevokeAndReturnSuccess()
    {
        // Arrange
        var token = "active-token";
        var refreshToken = RefreshToken.Create(Guid.NewGuid(), token, "jwt-id", DateTimeOffset.UtcNow.AddDays(1), "127.0.0.1", "device");
        var tokensList = new List<RefreshToken> { refreshToken };
        var dbSet = DbSetMockHelper.CreateMockDbSet(tokensList);

        _contextMock.Setup(x => x.RefreshTokens).Returns(dbSet);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new RevokeTokenCommandHandler(_contextMock.Object);

        // Act
        var result = await handler.Handle(new RevokeTokenCommand(token), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        refreshToken.IsRevoked.Should().BeTrue();
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNotFoundToken_ShouldReturnFailure()
    {
        // Arrange
        var tokensList = new List<RefreshToken>();
        var dbSet = DbSetMockHelper.CreateMockDbSet(tokensList);

        _contextMock.Setup(x => x.RefreshTokens).Returns(dbSet);

        var handler = new RevokeTokenCommandHandler(_contextMock.Object);

        // Act
        var result = await handler.Handle(new RevokeTokenCommand("unknown"), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Token not found.");
    }
}
