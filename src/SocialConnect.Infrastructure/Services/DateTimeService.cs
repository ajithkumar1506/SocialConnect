using SocialConnect.Application.Common.Interfaces;

namespace SocialConnect.Infrastructure.Services;

public class DateTimeService : IDateTime
{
    public DateTimeOffset Now => DateTimeOffset.UtcNow;
}
