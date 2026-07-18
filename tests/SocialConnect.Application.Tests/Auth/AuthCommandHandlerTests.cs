using Moq;
using FluentAssertions;
using SocialConnect.Application.Common.Exceptions;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Features.Auth.Commands.Login;
using SocialConnect.Application.Features.Auth.Commands.Register;
using SocialConnect.Domain.Entities.Users;
using SocialConnect.Domain.Repositories;
using SocialConnect.Domain.ValueObjects;

namespace SocialConnect.Application.Tests.Auth;

public class AuthCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IDateTime> _dateTimeMock;

    public AuthCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tokenServiceMock = new Mock<ITokenService>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _dateTimeMock = new Mock<IDateTime>();

        _dateTimeMock.Setup(d => d.Now).Returns(DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task Register_WithValidData_ShouldCreateUserAndReturnTokens()
    {
        // Arrange
        var command = new RegisterCommand("new@example.com", "Password123!", "new_user", "John", "Doe");
        
        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User)null!);

        _userRepositoryMock
            .Setup(x => x.GetByUserNameAsync(command.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User)null!);

        _passwordHasherMock
            .Setup(x => x.HashPassword(command.Password))
            .Returns("hashed_pwd");

        _tokenServiceMock
            .Setup(x => x.GenerateRefreshToken())
            .Returns("refresh_token_value");

        _tokenServiceMock
            .Setup(x => x.GenerateAccessToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<IList<string>>()))
            .Returns("access_token_value");

        var handler = new RegisterCommandHandler(
            _userRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _tokenServiceMock.Object,
            _passwordHasherMock.Object,
            _dateTimeMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be("access_token_value");
        result.RefreshToken.Should().Be("refresh_token_value");

        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Register_WithExistingEmail_ShouldThrowValidationException()
    {
        // Arrange
        var command = new RegisterCommand("existing@example.com", "Password123!", "user", "John", "Doe");
        var existingUser = User.Create(Email.Create(command.Email), "user", Password.Create("hashed"));

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        var handler = new RegisterCommandHandler(
            _userRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _tokenServiceMock.Object,
            _passwordHasherMock.Object,
            _dateTimeMock.Object);

        // Act
        Func<Task> action = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnTokens()
    {
        // Arrange
        var command = new LoginCommand("login@example.com", "Password123!");
        var user = User.Create(Email.Create(command.Email), "user", Password.Create("hashed"));

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.VerifyPassword(command.Password, user.PasswordHash.Hash))
            .Returns(true);

        _tokenServiceMock
            .Setup(x => x.GenerateAccessToken(user.Id, user.Email.Value, It.IsAny<IList<string>>()))
            .Returns("access_token_value");

        _tokenServiceMock
            .Setup(x => x.GenerateRefreshToken())
            .Returns("refresh_token_value");

        var handler = new LoginCommandHandler(
            _userRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _tokenServiceMock.Object,
            _passwordHasherMock.Object,
            _dateTimeMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be("access_token_value");
        result.RefreshToken.Should().Be("refresh_token_value");

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var command = new LoginCommand("login@example.com", "WrongPassword");
        var user = User.Create(Email.Create(command.Email), "user", Password.Create("hashed"));

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.VerifyPassword(command.Password, user.PasswordHash.Hash))
            .Returns(false);

        var handler = new LoginCommandHandler(
            _userRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _tokenServiceMock.Object,
            _passwordHasherMock.Object,
            _dateTimeMock.Object);

        // Act
        Func<Task> action = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
