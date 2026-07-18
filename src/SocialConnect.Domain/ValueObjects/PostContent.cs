using SocialConnect.Domain.Exceptions;

namespace SocialConnect.Domain.ValueObjects;

public class PostContent : Common.ValueObject
{
    public const int MaxLength = 5000;

    public string Value { get; }

    private PostContent(string value)
    {
        Value = value;
    }

    public static PostContent Create(string value)
    {
        if (value == null)
        {
            value = string.Empty;
        }

        value = value.Trim();

        if (value.Length > MaxLength)
        {
            throw new BusinessRuleViolationException(
                $"Post content cannot exceed {MaxLength} characters."
            );
        }

        return new PostContent(value);
    }

    public static implicit operator string?(PostContent? content) => content?.Value;
    public static implicit operator PostContent?(string? value) => value != null ? Create(value) : null;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
    {
        return Value;
    }
}
