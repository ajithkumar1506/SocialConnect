using FluentAssertions;
using Moq;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Features.Auth.Commands.Login;
using SocialConnect.Domain.Entities.Users;
using SocialConnect.Domain.Repositories;
using SocialConnect.Domain.ValueObjects;

namespace SocialConnect.Application.Tests.Users.Commands;

public class LoginUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IDateTime> _dateTimeMock;
    private readonly LoginCommandHandler _handler;

    public LoginUserCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tokenServiceMock = new Mock<ITokenService>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _dateTimeMock = new Mock<IDateTime>();

        _handler = new LoginCommandHandler(
            _userRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _tokenServiceMock.Object,
            _passwordHasherMock.Object,
            _dateTimeMock.Object
        );
    }

    [Fact]
    public async Task Handle_WhenCredentialsAreValid_ShouldReturnToken()
    {
        // Arrange
        var command = new LoginCommand("test@example.com", "Password123!");
        var user = User.Create(
            Email.Create("test@example.com"),
            "testuser",
            Password.Create("$2a$12$somevalidbcryptstringhere12345678901234567890123")
        );

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock.Setup(x => x.VerifyPassword(command.Password, user.PasswordHash.Hash))
            .Returns(true);

        _tokenServiceMock.Setup(x => x.GenerateAccessToken(user.Id, user.Email.Value, It.IsAny<IList<string>>()))
            .Returns("valid.jwt.token");

        _tokenServiceMock.Setup(x => x.GenerateRefreshToken())
            .Returns("refresh.token");

        _dateTimeMock.Setup(x => x.Now).Returns(DateTimeOffset.UtcNow);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Token.Should().Be("valid.jwt.token");
        result.RefreshToken.Should().Be("refresh.token");
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var command = new LoginCommand("nonexistent@example.com", "Password123!");

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        Func<Task> action = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
