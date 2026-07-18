using System.Security.Claims;
using FluentAssertions;
using Moq;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Features.Auth.Commands.ResetPassword;
using SocialConnect.Domain.Entities.Users;
using SocialConnect.Domain.ValueObjects;

namespace SocialConnect.Application.Tests.Users.Commands;

public class ResetPasswordCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;

    public ResetPasswordCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _tokenServiceMock = new Mock<ITokenService>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
    }

    [Fact]
    public async Task Handle_WithValidToken_ShouldResetPasswordAndReturnSuccess()
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

        _tokenServiceMock.Setup(x => x.ValidateTokenWithSecurityStamp("reset-token", "PasswordReset"))
            .Returns(principal);
        _contextMock.Setup(x => x.Users).Returns(dbSet);
        _passwordHasherMock.Setup(x => x.HashPassword("NewPassword123!")).Returns("new-hashed-password");
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new ResetPasswordCommandHandler(_contextMock.Object, _tokenServiceMock.Object, _passwordHasherMock.Object);

        // Act
        var result = await handler.Handle(new ResetPasswordCommand("reset-token", "NewPassword123!"), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.PasswordHash.Hash.Should().Be("new-hashed-password");
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
