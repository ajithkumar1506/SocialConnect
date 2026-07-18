using AutoMapper;
using SocialConnect.Application.Common.Mappings;
using SocialConnect.Domain.Entities.Messaging;

namespace SocialConnect.Application.Features.Messaging.DTOs;

public class MessageAttachmentDto : IMapFrom<MessageAttachment>
{
    public Guid Id { get; set; }
    public Guid MessageId { get; set; }
    public string FileUrl { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<MessageAttachment, MessageAttachmentDto>();
    }
}
