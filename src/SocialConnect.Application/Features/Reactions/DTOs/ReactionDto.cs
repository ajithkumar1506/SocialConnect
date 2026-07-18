using AutoMapper;
using SocialConnect.Application.Common.Mappings;
using SocialConnect.Domain.Entities.Posts;
using SocialConnect.Domain.Enums;

namespace SocialConnect.Application.Features.Reactions.DTOs;

public class ReactionDto : IMapFrom<Reaction>
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserFirstName { get; set; } = string.Empty;
    public string UserLastName { get; set; } = string.Empty;
    public string? UserProfileImageUrl { get; set; }
    public ReactionType ReactionType { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Reaction, ReactionDto>()
            .ForMember(d => d.UserName, opt => opt.MapFrom(s => s.User != null ? s.User.UserName : string.Empty))
            .ForMember(d => d.UserFirstName, opt => opt.MapFrom(s => s.User != null && s.User.Profile != null ? s.User.Profile.FirstName : string.Empty))
            .ForMember(d => d.UserLastName, opt => opt.MapFrom(s => s.User != null && s.User.Profile != null ? s.User.Profile.LastName : string.Empty))
            .ForMember(d => d.UserProfileImageUrl, opt => opt.MapFrom(s => s.User != null && s.User.Profile != null ? s.User.Profile.ProfileImageUrl : null));
    }
}
