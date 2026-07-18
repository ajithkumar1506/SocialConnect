using FluentAssertions;
using Moq;
using SocialConnect.Application.Common.Exceptions;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Features.Auth.Commands.Register;
using SocialConnect.Domain.Entities.Users;
using SocialConnect.Domain.Repositories;
using SocialConnect.Domain.ValueObjects;

namespace SocialConnect.Application.Tests.Users.Commands;

public class RegisterUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IDateTime> _dateTimeMock;
    private readonly RegisterCommandHandler _handler;

    public RegisterUserCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tokenServiceMock = new Mock<ITokenService>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _dateTimeMock = new Mock<IDateTime>();

        _handler = new RegisterCommandHandler(
            _userRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _tokenServiceMock.Object,
            _passwordHasherMock.Object,
            _dateTimeMock.Object
        );
    }

    [Fact]
    public async Task Handle_WhenEmailIsUnique_ShouldCreateUserAndReturnToken()
    {
        // Arrange
        var command = new RegisterCommand("test@example.com", "Password123!", "testuser", "John", "Doe");

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _userRepositoryMock.Setup(x => x.GetByUserNameAsync(command.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _passwordHasherMock.Setup(x => x.HashPassword(command.Password))
            .Returns("$2a$12$somevalidbcryptstringhere12345678901234567890123");

        _dateTimeMock.Setup(x => x.Now).Returns(DateTimeOffset.UtcNow);

        _tokenServiceMock.Setup(x => x.GenerateAccessToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<IList<string>>()))
            .Returns("valid.jwt.token");

        _tokenServiceMock.Setup(x => x.GenerateRefreshToken())
            .Returns("refresh.token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Token.Should().Be("valid.jwt.token");
        result.RefreshToken.Should().Be("refresh.token");

        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenEmailIsNotUnique_ShouldThrowValidationException()
    {
        // Arrange
        var command = new RegisterCommand("test@example.com", "Password123!", "testuser", "John", "Doe");

        var existingUser = User.Create(Email.Create("test@example.com"), "testuser", Password.Create("hash"));
        _userRepositoryMock.Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act
        Func<Task> action = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await action.Should().ThrowAsync<ValidationException>();
        exception.Which.Errors.Should().ContainKey("Email");

        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
