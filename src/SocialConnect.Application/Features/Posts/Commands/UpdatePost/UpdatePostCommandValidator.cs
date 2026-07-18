using FluentValidation;

namespace SocialConnect.Application.Features.Posts.Commands.UpdatePost;

public class UpdatePostCommandValidator : AbstractValidator<UpdatePostCommand>
{
    public UpdatePostCommandValidator()
    {
        RuleFor(v => v.PostId).NotEmpty();
        RuleFor(v => v.Content).NotEmpty().MaximumLength(5000);
    }
}
