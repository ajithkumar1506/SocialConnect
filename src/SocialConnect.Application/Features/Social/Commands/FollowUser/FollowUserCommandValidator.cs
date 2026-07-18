using FluentValidation;

namespace SocialConnect.Application.Features.Social.Commands.FollowUser;

public class FollowUserCommandValidator : AbstractValidator<FollowUserCommand>
{
    public FollowUserCommandValidator()
    {
        RuleFor(v => v.FollowingId).NotEmpty();
    }
}
