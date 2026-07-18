using MediatR;
using SocialConnect.Application.Features.Users.DTOs;

namespace SocialConnect.Application.Features.Users.Queries.GetUserProfile;

public record GetUserProfileQuery(Guid UserId) : IRequest<UserProfileDto?>;
