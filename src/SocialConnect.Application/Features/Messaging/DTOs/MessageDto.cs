using AutoMapper;
using SocialConnect.Application.Common.Mappings;
using SocialConnect.Domain.Entities.Messaging;
using SocialConnect.Domain.Enums;

namespace SocialConnect.Application.Features.Messaging.DTOs;

public class MessageDto : IMapFrom<Message>
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public Guid SenderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string? SenderProfileImageUrl { get; set; }
    public string Content { get; set; } = string.Empty;
    public MessageType MessageType { get; set; }
    public MessageStatus Status { get; set; }
    public Guid? ParentMessageId { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public List<MessageAttachmentDto> Attachments { get; set; } = new();

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Message, MessageDto>()
            .ForMember(d => d.SenderName, opt => opt.MapFrom(s => s.Sender != null ? s.Sender.UserName : string.Empty))
            .ForMember(d => d.SenderProfileImageUrl, opt => opt.MapFrom(s => s.Sender != null && s.Sender.Profile != null ? s.Sender.Profile.ProfileImageUrl : null))
            .ForMember(d => d.Attachments, opt => opt.MapFrom(s => s.Attachments));
    }
}
