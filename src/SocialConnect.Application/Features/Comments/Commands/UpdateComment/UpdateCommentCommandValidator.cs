using FluentValidation;

namespace SocialConnect.Application.Features.Comments.Commands.UpdateComment;

public class UpdateCommentCommandValidator : AbstractValidator<UpdateCommentCommand>
{
    public UpdateCommentCommandValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Comment content cannot be empty.")
            .MaximumLength(1000).WithMessage("Comment content cannot exceed 1000 characters.");
    }
}
