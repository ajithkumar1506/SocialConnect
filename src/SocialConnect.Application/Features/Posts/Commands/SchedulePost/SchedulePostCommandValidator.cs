using FluentValidation;

namespace SocialConnect.Application.Features.Posts.Commands.SchedulePost;

public class SchedulePostCommandValidator : AbstractValidator<SchedulePostCommand>
{
    public SchedulePostCommandValidator()
    {
        RuleFor(x => x.ScheduledAt)
            .GreaterThan(DateTimeOffset.UtcNow)
            .WithMessage("Scheduled time must be in the future.");
    }
}
