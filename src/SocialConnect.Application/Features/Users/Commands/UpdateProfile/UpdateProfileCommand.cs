using MediatR;
using SocialConnect.Application.Common.Models;
using SocialConnect.Application.Features.Users.DTOs;

namespace SocialConnect.Application.Features.Users.Commands.UpdateProfile;

public record UpdateProfileCommand(
    string FirstName,
    string LastName,
    string? Headline,
    string? Bio,
    string? Location,
    DateOnly? DateOfBirth
) : IRequest<Result<UserProfileDto>>;
