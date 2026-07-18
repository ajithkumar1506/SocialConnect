using System.Text.RegularExpressions;
using SocialConnect.Domain.Exceptions;

namespace SocialConnect.Domain.ValueObjects;

public class Email : Common.ValueObject
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new BusinessRuleViolationException("Email cannot be empty.");
        }

        value = value.Trim().ToLowerInvariant();

        if (!EmailRegex.IsMatch(value))
        {
            throw new BusinessRuleViolationException("Email format is invalid.");
        }

        return new Email(value);
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
