using FluentAssertions;
using SocialConnect.Domain.ValueObjects;

namespace SocialConnect.Domain.Tests.ValueObjects;

public class PasswordTests
{
    [Fact]
    public void Create_WithValidPasswordHash_ShouldReturnPasswordObject()
    {
        // Arrange
        string validHash = "$2a$12$somevalidbcryptstringhere12345678901234567890123";

        // Act
        var password = Password.Create(validHash);

        // Assert
        password.Hash.Should().Be(validHash);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyHash_ShouldThrowArgumentException(string? invalidHash)
    {
        // Act
        Action action = () => Password.Create(invalidHash!);

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage("*Password hash cannot be empty*");
    }
}
