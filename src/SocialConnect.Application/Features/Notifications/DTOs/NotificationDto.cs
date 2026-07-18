using SocialConnect.Domain.Enums;

namespace SocialConnect.Application.Features.Notifications.DTOs;

public record NotificationDto
{
    public Guid Id { get; init; }
    public Guid ActorId { get; init; }
    public string ActorName { get; init; } = string.Empty;
    public string? ActorProfileImageUrl { get; init; }
    public NotificationType Type { get; init; }
    public Guid? TargetId { get; init; }
    public string? TargetType { get; init; }
    public string Content { get; init; } = string.Empty;
    public bool IsRead { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
