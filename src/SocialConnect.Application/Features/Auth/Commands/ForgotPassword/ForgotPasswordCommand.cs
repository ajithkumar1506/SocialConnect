using MediatR;
using SocialConnect.Application.Common.Models;

namespace SocialConnect.Application.Features.Auth.Commands.ForgotPassword;

public record ForgotPasswordCommand(string Email) : IRequest<Result>;
