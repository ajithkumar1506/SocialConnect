using MediatR;
using SocialConnect.Application.Common.Models;

namespace SocialConnect.Application.Features.Auth.Commands.Logout;

public record LogoutCommand(string RefreshToken) : IRequest<Result>;
