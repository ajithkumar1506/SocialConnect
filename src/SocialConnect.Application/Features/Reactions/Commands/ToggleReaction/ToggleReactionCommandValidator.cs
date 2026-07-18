using FluentValidation;

namespace SocialConnect.Application.Features.Reactions.Commands.ToggleReaction;

public class ToggleReactionCommandValidator : AbstractValidator<ToggleReactionCommand>
{
    public ToggleReactionCommandValidator()
    {
        RuleFor(v => v.TargetId).NotEmpty();
        RuleFor(v => v.TargetType).IsInEnum();
        RuleFor(v => v.ReactionType).IsInEnum();
    }
}
