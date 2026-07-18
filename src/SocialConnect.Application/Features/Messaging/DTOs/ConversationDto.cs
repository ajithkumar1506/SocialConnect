using AutoMapper;
using SocialConnect.Application.Common.Mappings;
using SocialConnect.Domain.Entities.Messaging;
using SocialConnect.Domain.Enums;

namespace SocialConnect.Application.Features.Messaging.DTOs;

public class ConversationDto : IMapFrom<Conversation>
{
    public Guid Id { get; set; }
    public ConversationType Type { get; set; }
    public string? Name { get; set; }
    public string? ImageUrl { get; set; }
    public DateTimeOffset? LastMessageAt { get; set; }
    public int UnreadCount { get; set; }
    public List<ConversationMemberDto> Members { get; set; } = new();
    public MessageDto? LastMessage { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Conversation, ConversationDto>()
            .ForMember(d => d.Members, opt => opt.MapFrom(s => s.Members))
            .ForMember(d => d.LastMessage, opt => opt.Ignore())
            .ForMember(d => d.UnreadCount, opt => opt.Ignore());
    }
}
