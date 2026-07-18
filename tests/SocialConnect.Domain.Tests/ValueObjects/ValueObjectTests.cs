using FluentAssertions;
using SocialConnect.Domain.Exceptions;
using SocialConnect.Domain.ValueObjects;

namespace SocialConnect.Domain.Tests.ValueObjects;

public class ValueObjectTests
{
    [Fact]
    public void Create_WithValidMediaUrl_ShouldSucceed()
    {
        // Act
        var mediaUrl = MediaUrl.Create("https://example.com/image.png");

        // Assert
        mediaUrl.Value.Should().Be("https://example.com/image.png");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyMediaUrl_ShouldThrowException(string? url)
    {
        // Act
        Action action = () => MediaUrl.Create(url!);

        // Assert
        action.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("Media URL cannot be empty.");
    }

    [Theory]
    [InlineData("ftp://example.com/image.png")]
    [InlineData("invalid-url")]
    [InlineData("/relative/path")]
    public void Create_WithInvalidMediaUrlFormat_ShouldThrowException(string url)
    {
        // Act
        Action action = () => MediaUrl.Create(url);

        // Assert
        action.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("Media URL must be a valid absolute HTTP/HTTPS URL.");
    }

    [Fact]
    public void Create_WithValidPostContent_ShouldSucceed()
    {
        // Act
        var content = PostContent.Create("Hello World");

        // Assert
        content.Value.Should().Be("Hello World");
    }

    [Fact]
    public void Create_WithNullPostContent_ShouldReturnEmptyStringContent()
    {
        // Act
        var content = PostContent.Create(null!);

        // Assert
        content.Value.Should().Be(string.Empty);
    }

    [Fact]
    public void Create_WithTooLongPostContent_ShouldThrowException()
    {
        // Arrange
        var longString = new string('a', PostContent.MaxLength + 1);

        // Act
        Action action = () => PostContent.Create(longString);

        // Assert
        action.Should().Throw<BusinessRuleViolationException>()
            .WithMessage($"Post content cannot exceed {PostContent.MaxLength} characters.");
    }
}
