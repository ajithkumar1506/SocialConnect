using MediatR;
using SocialConnect.Application.Common.Models;

namespace SocialConnect.Application.Features.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(string Token, string RefreshToken) : IRequest<AuthResult>;
