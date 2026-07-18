using MediatR;
using SocialConnect.Application.Common.Models;

namespace SocialConnect.Application.Features.Social.Commands.MuteUser;

public record MuteUserCommand(Guid MutedId) : IRequest<Result<bool>>;
