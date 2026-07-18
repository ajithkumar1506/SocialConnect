using FluentAssertions;
using Moq;
using SocialConnect.Application.BackgroundJobs;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Common.Models;
using SocialConnect.Application.Features.Auth.Commands.ForgotPassword;
using SocialConnect.Domain.Entities.Users;
using SocialConnect.Domain.ValueObjects;

namespace SocialConnect.Application.Tests.Users.Commands;

public class ForgotPasswordCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<IBackgroundJobService> _backgroundJobServiceMock;
    private readonly ClientSettings _clientSettings;

    public ForgotPasswordCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _tokenServiceMock = new Mock<ITokenService>();
        _backgroundJobServiceMock = new Mock<IBackgroundJobService>();
        _clientSettings = new ClientSettings { ResetPasswordUrl = "http://localhost:5000/api/v1/auth/reset-password" };
    }

    [Fact]
    public async Task Handle_WithExistingUser_ShouldSendEmailAndReturnSuccess()
    {
        // Arrange
        var user = User.Create(Email.Create("user@example.com"), "username", Password.Create("hashed"));
        var usersList = new List<User> { user };
        var dbSet = DbSetMockHelper.CreateMockDbSet(usersList);

        _contextMock.Setup(x => x.Users).Returns(dbSet);
        _tokenServiceMock.Setup(x => x.GeneratePasswordResetToken(user.Id, user.Email.Value, user.SecurityStamp))
            .Returns("reset-token");

        var handler = new ForgotPasswordCommandHandler(_contextMock.Object, _tokenServiceMock.Object, _backgroundJobServiceMock.Object, _clientSettings);

        // Act
        var result = await handler.Handle(new ForgotPasswordCommand("user@example.com"), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _backgroundJobServiceMock.Verify(x => x.Enqueue<EmailSendingJob>(It.IsAny<System.Linq.Expressions.Expression<System.Func<EmailSendingJob, System.Threading.Tasks.Task>>>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldReturnSuccessWithoutSendingEmail()
    {
        // Arrange
        var usersList = new List<User>();
        var dbSet = DbSetMockHelper.CreateMockDbSet(usersList);

        _contextMock.Setup(x => x.Users).Returns(dbSet);

        var handler = new ForgotPasswordCommandHandler(_contextMock.Object, _tokenServiceMock.Object, _backgroundJobServiceMock.Object, _clientSettings);

        // Act
        var result = await handler.Handle(new ForgotPasswordCommand("unknown@example.com"), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _backgroundJobServiceMock.Verify(x => x.Enqueue<EmailSendingJob>(It.IsAny<System.Linq.Expressions.Expression<System.Func<EmailSendingJob, System.Threading.Tasks.Task>>>()), Times.Never);
    }
}
