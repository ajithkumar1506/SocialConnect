using MediatR;
using SocialConnect.Application.Common.Models;

namespace SocialConnect.Application.Features.Auth.Commands.VerifyEmail;

public record VerifyEmailCommand(string Token) : IRequest<Result>;
