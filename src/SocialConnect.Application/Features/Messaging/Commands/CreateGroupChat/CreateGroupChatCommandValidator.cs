using FluentValidation;

namespace SocialConnect.Application.Features.Messaging.Commands.CreateGroupChat;

public class CreateGroupChatCommandValidator : AbstractValidator<CreateGroupChatCommand>
{
    public CreateGroupChatCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Group name is required.")
            .MaximumLength(100).WithMessage("Group name cannot exceed 100 characters.");
    }
}
