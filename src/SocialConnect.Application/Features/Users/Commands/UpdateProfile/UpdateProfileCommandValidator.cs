using FluentValidation;

namespace SocialConnect.Application.Features.Users.Commands.UpdateProfile;

public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(50).WithMessage("First name must not exceed 50 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(50).WithMessage("Last name must not exceed 50 characters.");

        RuleFor(x => x.Headline)
            .MaximumLength(200).WithMessage("Headline must not exceed 200 characters.");

        RuleFor(x => x.Bio)
            .MaximumLength(1000).WithMessage("Bio must not exceed 1000 characters.");

        RuleFor(x => x.Location)
            .MaximumLength(100).WithMessage("Location must not exceed 100 characters.");

        RuleFor(x => x.DateOfBirth)
            .Must(dob => !dob.HasValue || dob.Value < DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("Date of birth must be in the past.");
    }
}
