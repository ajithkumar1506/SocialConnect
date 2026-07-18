using FluentValidation;

namespace SocialConnect.Application.Features.Social.Commands.UnfollowUser;

public class UnfollowUserCommandValidator : AbstractValidator<UnfollowUserCommand>
{
    public UnfollowUserCommandValidator()
    {
        RuleFor(v => v.FollowingId).NotEmpty();
    }
}
