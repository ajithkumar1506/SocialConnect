using Moq;
using SocialConnect.Application.BackgroundJobs;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Features.Notifications.DTOs;
using SocialConnect.Application.Tests.Users.Commands;
using SocialConnect.Domain.Entities.Notifications;
using SocialConnect.Domain.Entities.Users;
using SocialConnect.Domain.Enums;
using SocialConnect.Domain.Repositories;
using SocialConnect.Domain.ValueObjects;

namespace SocialConnect.Application.Tests.BackgroundJobs;

public class BackgroundJobTests
{
    [Fact]
    public async Task EmailSendingJob_SendPasswordResetEmailAsync_ShouldCallEmailService()
    {
        // Arrange
        var emailServiceMock = new Mock<IEmailService>();
        var job = new EmailSendingJob(emailServiceMock.Object);

        // Act
        await job.SendPasswordResetEmailAsync("test@example.com", "reset-token");

        // Assert
        emailServiceMock.Verify(x => x.SendPasswordResetEmailAsync("test@example.com", "reset-token", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EmailSendingJob_SendVerificationEmailAsync_ShouldCallEmailService()
    {
        // Arrange
        var emailServiceMock = new Mock<IEmailService>();
        var job = new EmailSendingJob(emailServiceMock.Object);

        // Act
        await job.SendVerificationEmailAsync("test@example.com", "verify-token");

        // Assert
        emailServiceMock.Verify(x => x.SendVerificationEmailAsync("test@example.com", "verify-token", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task NotificationProcessingJob_ProcessNotificationAsync_ShouldPersistAndPushNotification()
    {
        // Arrange
        var contextMock = new Mock<IApplicationDbContext>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var notificationServiceMock = new Mock<INotificationService>();

        var userId = Guid.NewGuid();
        var actor = User.Create(Email.Create("actor@example.com"), "actor", Password.Create("hash"));
        var actorId = actor.Id;
        var actorProfile = UserProfile.Create(actorId, "ActorFirst", "ActorLast");
        actor.SetProfile(actorProfile);

        var notificationsList = new List<Notification>();
        var notificationsDbSet = DbSetMockHelper.CreateMockDbSet(notificationsList);
        var usersDbSet = DbSetMockHelper.CreateMockDbSet(new List<User> { actor });

        contextMock.Setup(x => x.Notifications).Returns(notificationsDbSet);
        contextMock.Setup(x => x.Users).Returns(usersDbSet);

        var job = new NotificationProcessingJob(
            contextMock.Object,
            unitOfWorkMock.Object,
            notificationServiceMock.Object
        );

        // Act
        await job.ProcessNotificationAsync(
            userId,
            actorId,
            NotificationType.NewComment,
            "User commented on your post",
            Guid.NewGuid(),
            "Post"
        );

        // Assert
        contextMock.Verify(x => x.Notifications.Add(It.IsAny<Notification>()), Times.Once);
        unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        notificationServiceMock.Verify(x => x.SendNotificationAsync(
            userId,
            It.Is<NotificationDto>(dto => dto.ActorName == "ActorFirst ActorLast" && dto.Content == "User commented on your post"),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }
}
