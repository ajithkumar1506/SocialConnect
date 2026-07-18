using MediatR;
using SocialConnect.Application.Features.Users.DTOs;

namespace SocialConnect.Application.Features.Users.Queries.GetUserById;

public record GetUserByIdQuery(Guid UserId) : IRequest<UserDto?>;
