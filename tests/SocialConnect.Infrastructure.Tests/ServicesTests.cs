using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using FluentAssertions;
using Moq;
using SocialConnect.Infrastructure.Services;

namespace SocialConnect.Infrastructure.Tests;

public class ServicesTests
{
    private readonly TokenService _tokenService;
    private readonly PasswordHasher _passwordHasher;
    private readonly Mock<IConfiguration> _configurationMock;

    public ServicesTests()
    {
        _configurationMock = new Mock<IConfiguration>();
        _configurationMock.Setup(x => x["JwtSettings:Secret"]).Returns("super_secret_key_which_is_at_least_32_bytes_long_12345");
        _configurationMock.Setup(x => x["JwtSettings:Issuer"]).Returns("SocialConnect");
        _configurationMock.Setup(x => x["JwtSettings:Audience"]).Returns("SocialConnectUsers");
        _configurationMock.Setup(x => x["JwtSettings:DurationInMinutes"]).Returns("60");

        _tokenService = new TokenService(_configurationMock.Object);
        _passwordHasher = new PasswordHasher();
    }

    [Fact]
    public void GenerateAccessToken_ShouldReturnValidJwtToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "test@example.com";
        var roles = new List<string> { "User", "Admin" };

        // Act
        var tokenString = _tokenService.GenerateAccessToken(userId, email, roles);

        // Assert
        tokenString.Should().NotBeNullOrEmpty();

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(tokenString);

        jwtToken.Subject.Should().Be(userId.ToString());
        jwtToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value.Should().Be(email);
        jwtToken.Claims.Where(c => c.Type == System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).Should().Contain(roles);
    }

    [Fact]
    public void GenerateRefreshToken_ShouldReturnCryptographicallySecureString()
    {
        // Act
        var token1 = _tokenService.GenerateRefreshToken();
        var token2 = _tokenService.GenerateRefreshToken();

        // Assert
        token1.Should().NotBeNullOrEmpty();
        token2.Should().NotBeNullOrEmpty();
        token1.Should().NotBe(token2);
    }

    [Fact]
    public void PasswordHasher_HashAndVerify_ShouldBeSuccessful()
    {
        // Arrange
        var password = "SecurePassword123!";

        // Act
        var hash = _passwordHasher.HashPassword(password);
        var verifySuccess = _passwordHasher.VerifyPassword(password, hash);
        var verifyFail = _passwordHasher.VerifyPassword("WrongPassword", hash);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        verifySuccess.Should().BeTrue();
        verifyFail.Should().BeFalse();
    }
}
