using MediatR;
using SocialConnect.Application.Common.Models;

namespace SocialConnect.Application.Features.Social.Commands.BlockUser;

public record BlockUserCommand(Guid BlockedId) : IRequest<Result<bool>>;
