using FluentAssertions;
using SocialConnect.Application.Features.Auth.Commands.Login;

namespace SocialConnect.Application.Tests.Users.Commands;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator;

    public LoginCommandValidatorTests()
    {
        _validator = new LoginCommandValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new LoginCommand("test@example.com", "Password123!");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "Password123!")]
    [InlineData("notanemail", "Password123!")]
    [InlineData("test@example.com", "")]
    public void Validate_WithInvalidCommand_ShouldHaveErrors(string email, string password)
    {
        // Arrange
        var command = new LoginCommand(email, password);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }
}
