using MediatR;
using SocialConnect.Application.Common.Models;

namespace SocialConnect.Application.Features.Auth.Commands.Register;

public record RegisterCommand(
    string Email,
    string Password,
    string Username,
    string FirstName,
    string LastName
) : IRequest<AuthResult>;
