using MediatR;
using SocialConnect.Application.Common.Models;
using SocialConnect.Domain.Enums;

namespace SocialConnect.Application.Features.Reactions.Commands.ToggleReaction;

public record ToggleReactionCommand(Guid TargetId, TargetType TargetType, ReactionType ReactionType)
    : IRequest<Result<bool>>; // Returns true if added/updated, false if removed
