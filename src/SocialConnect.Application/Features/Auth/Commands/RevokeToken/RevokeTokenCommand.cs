using MediatR;
using SocialConnect.Application.Common.Models;

namespace SocialConnect.Application.Features.Auth.Commands.RevokeToken;

public record RevokeTokenCommand(string Token) : IRequest<Result>;
