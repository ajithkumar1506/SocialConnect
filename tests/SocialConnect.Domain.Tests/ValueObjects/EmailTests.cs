using FluentAssertions;
using SocialConnect.Domain.Exceptions;
using SocialConnect.Domain.ValueObjects;

namespace SocialConnect.Domain.Tests.ValueObjects;

public class EmailTests
{
    [Fact]
    public void Create_WithValidEmail_ShouldReturnEmailObject()
    {
        // Arrange
        string validEmail = "test@example.com";

        // Act
        var email = Email.Create(validEmail);

        // Assert
        email.Value.Should().Be(validEmail);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyEmail_ShouldThrowException(string? invalidEmail)
    {
        // Act
        Action action = () => Email.Create(invalidEmail!);

        // Assert
        action.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("Email cannot be empty.");
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("test@")]
    [InlineData("@example.com")]
    [InlineData("test@example")]
    public void Create_WithInvalidFormat_ShouldThrowException(string invalidEmail)
    {
        // Act
        Action action = () => Email.Create(invalidEmail);

        // Assert
        action.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("Email format is invalid.");
    }

    [Fact]
    public void Emails_WithSameValue_ShouldBeEqual()
    {
        // Arrange
        var email1 = Email.Create("test@example.com");
        var email2 = Email.Create("test@example.com");

        // Act & Assert
        email1.Should().Be(email2);
        (email1 == email2).Should().BeTrue();
    }

    [Fact]
    public void Emails_WithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var email1 = Email.Create("test1@example.com");
        var email2 = Email.Create("test2@example.com");

        // Act & Assert
        email1.Should().NotBe(email2);
        (email1 != email2).Should().BeTrue();
    }
}
