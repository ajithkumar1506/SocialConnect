using FluentAssertions;
using SocialConnect.Domain.Entities.Users;
using SocialConnect.Domain.Events.Users;
using SocialConnect.Domain.ValueObjects;

namespace SocialConnect.Domain.Tests.Entities;

public class UserTests
{
    [Fact]
    public void Create_WithValidData_ShouldReturnUserAndRaiseEvent()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var password = Password.Create("$2a$12$somevalidbcryptstringhere12345678901234567890123");
        var username = "testuser";

        // Act
        var user = User.Create(email, username, password);

        // Assert
        user.Email.Should().Be(email);
        user.UserName.Should().Be(username);
        user.PasswordHash.Should().Be(password);
        user.IsActive.Should().BeTrue();
        user.EmailVerified.Should().BeFalse();

        // Check if event was raised
        user.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<UserRegisteredDomainEvent>()
            .Which.UserId.Should().Be(user.Id);
    }

    [Fact]
    public void VerifyEmail_ShouldSetVerifiedAndRaiseEvent()
    {
        // Arrange
        var user = User.Create(Email.Create("test@example.com"), "user", Password.Create("$2a$12$somevalidbcryptstringhere12345678901234567890123"));
        user.ClearDomainEvents();

        // Act
        user.VerifyEmail();

        // Assert
        user.EmailVerified.Should().BeTrue();
        user.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<UserVerifiedDomainEvent>();
    }

    [Fact]
    public void UpdatePassword_ShouldHashPasswordAndChangeSecurityStamp()
    {
        // Arrange
        var user = User.Create(Email.Create("test@example.com"), "user", Password.Create("$2a$12$somevalidbcryptstringhere12345678901234567890123"));
        var oldStamp = user.SecurityStamp;
        var newPassword = Password.Create("$2a$12$someotherbcryptstringhere12345678901234567890123");

        // Act
        user.UpdatePassword(newPassword);

        // Assert
        user.PasswordHash.Should().Be(newPassword);
        user.SecurityStamp.Should().NotBe(oldStamp);
    }

    [Fact]
    public void RecordAccessFailed_ShouldIncrementCount()
    {
        // Arrange
        var user = User.Create(Email.Create("test@example.com"), "user", Password.Create("$2a$12$somevalidbcryptstringhere12345678901234567890123"));

        // Act
        user.RecordAccessFailed();

        // Assert
        user.AccessFailedCount.Should().Be(1);
    }

    [Fact]
    public void ResetAccessFailedCount_ShouldClearCountAndLockout()
    {
        // Arrange
        var user = User.Create(Email.Create("test@example.com"), "user", Password.Create("$2a$12$somevalidbcryptstringhere12345678901234567890123"));
        user.RecordAccessFailed();
        user.Lockout(DateTimeOffset.UtcNow.AddMinutes(5));

        // Act
        user.ResetAccessFailedCount();

        // Assert
        user.AccessFailedCount.Should().Be(0);
        user.LockoutEnd.Should().BeNull();
    }

    [Fact]
    public void Deactivate_ShouldSetActiveFalse()
    {
        // Arrange
        var user = User.Create(Email.Create("test@example.com"), "user", Password.Create("$2a$12$somevalidbcryptstringhere12345678901234567890123"));

        // Act
        user.Deactivate();

        // Assert
        user.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_ShouldSetActiveTrue()
    {
        // Arrange
        var user = User.Create(Email.Create("test@example.com"), "user", Password.Create("$2a$12$somevalidbcryptstringhere12345678901234567890123"));
        user.Deactivate();

        // Act
        user.Activate();

        // Assert
        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public void RefreshTokens_AddAndRemove_ShouldWork()
    {
        // Arrange
        var user = User.Create(Email.Create("test@example.com"), "user", Password.Create("$2a$12$somevalidbcryptstringhere12345678901234567890123"));
        var token = RefreshToken.Create(user.Id, "token123", "jwt123", DateTimeOffset.UtcNow.AddDays(7), "127.0.0.1", "Web");

        // Act
        user.AddRefreshToken(token);

        // Assert
        user.RefreshTokens.Should().Contain(token);

        // Act
        user.RemoveRefreshToken(token);

        // Assert
        user.RefreshTokens.Should().NotContain(token);
    }
}
