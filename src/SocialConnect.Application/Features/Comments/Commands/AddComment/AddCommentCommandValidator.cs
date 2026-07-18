using FluentValidation;

namespace SocialConnect.Application.Features.Comments.Commands.AddComment;

public class AddCommentCommandValidator : AbstractValidator<AddCommentCommand>
{
    public AddCommentCommandValidator()
    {
        RuleFor(v => v.PostId).NotEmpty();
        RuleFor(v => v.Content).NotEmpty().MaximumLength(1000);
    }
}
