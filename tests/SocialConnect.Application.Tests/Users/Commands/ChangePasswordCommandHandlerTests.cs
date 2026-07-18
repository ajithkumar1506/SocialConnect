using FluentAssertions;
using Moq;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Features.Auth.Commands.ChangePassword;
using SocialConnect.Domain.Entities.Users;
using SocialConnect.Domain.ValueObjects;

namespace SocialConnect.Application.Tests.Users.Commands;

public class ChangePasswordCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;

    public ChangePasswordCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
    }

    [Fact]
    public async Task Handle_WithValidCurrentPassword_ShouldChangePasswordAndReturnSuccess()
    {
        // Arrange
        var user = User.Create(Email.Create("user@example.com"), "username", Password.Create("hashed"));
        var usersList = new List<User> { user };
        var dbSet = DbSetMockHelper.CreateMockDbSet(usersList);

        _currentUserServiceMock.Setup(x => x.UserId).Returns(user.Id);
        _contextMock.Setup(x => x.Users).Returns(dbSet);
        _passwordHasherMock.Setup(x => x.VerifyPassword("CurrentPassword", "hashed")).Returns(true);
        _passwordHasherMock.Setup(x => x.HashPassword("NewPassword123!")).Returns("new-hashed-password");
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new ChangePasswordCommandHandler(_contextMock.Object, _currentUserServiceMock.Object, _passwordHasherMock.Object);

        // Act
        var result = await handler.Handle(new ChangePasswordCommand("CurrentPassword", "NewPassword123!"), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.PasswordHash.Hash.Should().Be("new-hashed-password");
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidCurrentPassword_ShouldReturnFailure()
    {
        // Arrange
        var user = User.Create(Email.Create("user@example.com"), "username", Password.Create("hashed"));
        var usersList = new List<User> { user };
        var dbSet = DbSetMockHelper.CreateMockDbSet(usersList);

        _currentUserServiceMock.Setup(x => x.UserId).Returns(user.Id);
        _contextMock.Setup(x => x.Users).Returns(dbSet);
        _passwordHasherMock.Setup(x => x.VerifyPassword("WrongPassword", "hashed")).Returns(false);

        var handler = new ChangePasswordCommandHandler(_contextMock.Object, _currentUserServiceMock.Object, _passwordHasherMock.Object);

        // Act
        var result = await handler.Handle(new ChangePasswordCommand("WrongPassword", "NewPassword123!"), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("The current password provided is incorrect.");
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
