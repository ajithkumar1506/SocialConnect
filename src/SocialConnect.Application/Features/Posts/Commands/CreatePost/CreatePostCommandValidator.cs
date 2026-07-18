using FluentValidation;

namespace SocialConnect.Application.Features.Posts.Commands.CreatePost;

public class CreatePostCommandValidator : AbstractValidator<CreatePostCommand>
{
    public CreatePostCommandValidator()
    {
        RuleFor(v => v.PostType).IsInEnum();
        RuleFor(v => v.Status).IsInEnum();

        RuleFor(v => v)
            .Must(v =>
                !string.IsNullOrWhiteSpace(v.Content)
                || (v.MediaItems != null && v.MediaItems.Any())
            )
            .WithMessage("A post must contain either content or media.");
    }
}
