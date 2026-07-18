using AutoMapper;
using SocialConnect.Application.Common.Mappings;
using SocialConnect.Domain.Entities.Messaging;
using SocialConnect.Domain.Enums;

namespace SocialConnect.Application.Features.Messaging.DTOs;

public class ConversationMemberDto : IMapFrom<ConversationMember>
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }
    public ConversationMemberRole Role { get; set; }
    public bool IsMuted { get; set; }
    public DateTimeOffset JoinedAt { get; set; }
    public DateTimeOffset? LastReadAt { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<ConversationMember, ConversationMemberDto>()
            .ForMember(d => d.UserName, opt => opt.MapFrom(s => s.User != null ? s.User.UserName : string.Empty))
            .ForMember(d => d.FirstName, opt => opt.MapFrom(s => s.User != null && s.User.Profile != null ? s.User.Profile.FirstName : string.Empty))
            .ForMember(d => d.LastName, opt => opt.MapFrom(s => s.User != null && s.User.Profile != null ? s.User.Profile.LastName : string.Empty))
            .ForMember(d => d.ProfileImageUrl, opt => opt.MapFrom(s => s.User != null && s.User.Profile != null ? s.User.Profile.ProfileImageUrl : null));
    }
}
