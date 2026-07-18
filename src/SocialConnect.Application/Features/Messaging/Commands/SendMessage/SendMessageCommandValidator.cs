using FluentValidation;

namespace SocialConnect.Application.Features.Messaging.Commands.SendMessage;

public class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageCommandValidator()
    {
        RuleFor(v => v.ConversationId).NotEmpty();
        RuleFor(v => v.Content).NotEmpty().MaximumLength(2000);
        RuleFor(v => v.Type).IsInEnum();
    }
}
