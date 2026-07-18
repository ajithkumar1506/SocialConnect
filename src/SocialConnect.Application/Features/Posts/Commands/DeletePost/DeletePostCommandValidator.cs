using FluentValidation;

namespace SocialConnect.Application.Features.Posts.Commands.DeletePost;

public class DeletePostCommandValidator : AbstractValidator<DeletePostCommand>
{
    public DeletePostCommandValidator()
    {
        RuleFor(v => v.PostId).NotEmpty();
    }
}
