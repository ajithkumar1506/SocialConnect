using Microsoft.EntityFrameworkCore;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Features.Notifications.DTOs;
using SocialConnect.Domain.Entities.Notifications;
using SocialConnect.Domain.Enums;
using SocialConnect.Domain.Repositories;

namespace SocialConnect.Application.BackgroundJobs;

public class NotificationProcessingJob
{
    private readonly IApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;

    public NotificationProcessingJob(
        IApplicationDbContext context,
        IUnitOfWork unitOfWork,
        INotificationService notificationService
    )
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
    }

    public async Task ProcessNotificationAsync(
        Guid userId,
        Guid actorId,
        NotificationType type,
        string content,
        Guid? targetId = null,
        string? targetType = null
    )
    {
        var notification = Notification.Create(userId, actorId, type, content, targetId, targetType);

        _context.Notifications.Add(notification);
        await _unitOfWork.SaveChangesAsync(CancellationToken.None);

        var actor = await _context.Users
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Id == actorId, CancellationToken.None);

        var dto = new NotificationDto
        {
            Id = notification.Id,
            ActorId = actorId,
            ActorName = actor?.Profile != null
                ? $"{actor.Profile.FirstName} {actor.Profile.LastName}".Trim()
                : "Someone",
            ActorProfileImageUrl = actor?.Profile?.ProfileImageUrl,
            Type = type,
            TargetId = targetId,
            TargetType = targetType,
            Content = content,
            IsRead = false,
            CreatedAt = notification.CreatedAt
        };

        await _notificationService.SendNotificationAsync(userId, dto, CancellationToken.None);
    }
}
