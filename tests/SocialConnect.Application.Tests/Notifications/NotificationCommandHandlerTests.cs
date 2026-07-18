using Moq;
using FluentAssertions;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Features.Notifications.Queries.GetUserNotifications;
using SocialConnect.Application.Features.Notifications.Commands.MarkAllNotificationsRead;
using SocialConnect.Domain.Entities.Notifications;
using SocialConnect.Domain.Repositories;
using SocialConnect.Domain.Enums;

namespace SocialConnect.Application.Tests.Notifications;

public class NotificationCommandHandlerTests
{
    private readonly Mock<INotificationRepository> _notificationRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    public NotificationCommandHandlerTests()
    {
        _notificationRepositoryMock = new Mock<INotificationRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
    }

    [Fact]
    public async Task GetUserNotifications_WhenUserLoggedIn_ShouldReturnNotifications()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

        var notifications = new List<Notification>
        {
            Notification.Create(userId, Guid.NewGuid(), NotificationType.NewFollower, "New Follower")
        };

        _notificationRepositoryMock
            .Setup(x => x.GetUserNotificationsAsync(userId, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(notifications);

        var handler = new GetUserNotificationsQueryHandler(
            _notificationRepositoryMock.Object,
            _currentUserServiceMock.Object);

        var query = new GetUserNotificationsQuery(1, 10);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().Content.Should().Be("New Follower");
    }

    [Fact]
    public async Task GetUserNotifications_WhenUserNotLoggedIn_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        _currentUserServiceMock.Setup(x => x.UserId).Returns((Guid?)null);

        var handler = new GetUserNotificationsQueryHandler(
            _notificationRepositoryMock.Object,
            _currentUserServiceMock.Object);

        var query = new GetUserNotificationsQuery(1, 10);

        // Act
        Func<Task> action = async () => await handler.Handle(query, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("User must be logged in to view notifications.");
    }

    [Fact]
    public async Task MarkAllNotificationsRead_WhenUserLoggedIn_ShouldMarkAsReadAndSave()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

        var handler = new MarkAllNotificationsReadCommandHandler(
            _notificationRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object);

        var command = new MarkAllNotificationsReadCommand();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        _notificationRepositoryMock.Verify(x => x.MarkAllAsReadAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
