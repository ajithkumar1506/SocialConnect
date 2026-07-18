namespace SocialConnect.Application.Common.Models;

public class AuthResult
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTimeOffset RefreshTokenExpiration { get; set; }
}
