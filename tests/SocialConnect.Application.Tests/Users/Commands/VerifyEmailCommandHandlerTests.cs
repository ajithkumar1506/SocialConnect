using System.Security.Claims;
using FluentAssertions;
using Moq;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Features.Auth.Commands.VerifyEmail;
using SocialConnect.Domain.Entities.Users;
using SocialConnect.Domain.ValueObjects;

namespace SocialConnect.Application.Tests.Users.Commands;

public class VerifyEmailCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<ITokenService> _tokenServiceMock;

    public VerifyEmailCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _tokenServiceMock = new Mock<ITokenService>();
    }

    [Fact]
    public async Task Handle_WithValidToken_ShouldVerifyEmailAndReturnSuccess()
    {
        // Arrange
        var user = User.Create(Email.Create("user@example.com"), "username", Password.Create("hashed"));
        var usersList = new List<User> { user };
        var dbSet = DbSetMockHelper.CreateMockDbSet(usersList);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim("SecurityStamp", user.SecurityStamp)
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        _tokenServiceMock.Setup(x => x.ValidateTokenWithSecurityStamp("valid-token", "EmailVerification"))
            .Returns(principal);
        _contextMock.Setup(x => x.Users).Returns(dbSet);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new VerifyEmailCommandHandler(_contextMock.Object, _tokenServiceMock.Object);

        // Act
        var result = await handler.Handle(new VerifyEmailCommand("valid-token"), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.EmailVerified.Should().BeTrue();
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithMismatchedSecurityStamp_ShouldReturnFailure()
    {
        // Arrange
        var user = User.Create(Email.Create("user@example.com"), "username", Password.Create("hashed"));
        var usersList = new List<User> { user };
        var dbSet = DbSetMockHelper.CreateMockDbSet(usersList);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim("SecurityStamp", "old-stamp")
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        _tokenServiceMock.Setup(x => x.ValidateTokenWithSecurityStamp("token", "EmailVerification"))
            .Returns(principal);
        _contextMock.Setup(x => x.Users).Returns(dbSet);

        var handler = new VerifyEmailCommandHandler(_contextMock.Object, _tokenServiceMock.Object);

        // Act
        var result = await handler.Handle(new VerifyEmailCommand("token"), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Verification token has expired or is invalid.");
    }
}
