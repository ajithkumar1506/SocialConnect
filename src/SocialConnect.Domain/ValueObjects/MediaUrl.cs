using SocialConnect.Domain.Exceptions;

namespace SocialConnect.Domain.ValueObjects;

public class MediaUrl : Common.ValueObject
{
    public string Value { get; }

    private MediaUrl(string value)
    {
        Value = value;
    }

    public static MediaUrl Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new BusinessRuleViolationException("Media URL cannot be empty.");
        }

        if (
            !Uri.TryCreate(value, UriKind.Absolute, out var uriResult)
            || (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps)
        )
        {
            throw new BusinessRuleViolationException(
                "Media URL must be a valid absolute HTTP/HTTPS URL."
            );
        }

        return new MediaUrl(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
    {
        return Value;
    }
}
