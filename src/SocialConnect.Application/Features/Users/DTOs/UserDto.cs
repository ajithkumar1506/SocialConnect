using AutoMapper;
using SocialConnect.Application.Common.Mappings;
using SocialConnect.Domain.Entities.Users;

namespace SocialConnect.Application.Features.Users.DTOs;

public class UserDto : IMapFrom<User>
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool EmailVerified { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public UserProfileDto? Profile { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<User, UserDto>()
            .ForMember(d => d.Email, opt => opt.MapFrom(s => s.Email.Value));
    }
}
