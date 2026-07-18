using FluentValidation;

namespace SocialConnect.Application.Features.Messaging.Commands.CreateConversation;

public class CreateConversationCommandValidator : AbstractValidator<CreateConversationCommand>
{
    public CreateConversationCommandValidator()
    {
        RuleFor(x => x.RecipientId)
            .NotEmpty().WithMessage("Recipient ID is required.");
    }
}
