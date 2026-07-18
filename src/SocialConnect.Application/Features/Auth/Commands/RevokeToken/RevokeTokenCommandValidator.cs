using FluentValidation;

namespace SocialConnect.Application.Features.Auth.Commands.RevokeToken;

public class RevokeTokenCommandValidator : AbstractValidator<RevokeTokenCommand>
{
    public RevokeTokenCommandValidator()
    {
        RuleFor(x => x.Token).NotEmpty().WithMessage("Token is required.");
    }
}
