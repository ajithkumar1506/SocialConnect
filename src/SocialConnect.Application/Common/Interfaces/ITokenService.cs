using System.Security.Claims;

namespace SocialConnect.Application.Common.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(Guid userId, string email, IList<string> roles);
    string GenerateRefreshToken();
    string GenerateEmailVerificationToken(Guid userId, string email, string securityStamp);
    string GeneratePasswordResetToken(Guid userId, string email, string securityStamp);
    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    ClaimsPrincipal? ValidateTokenWithSecurityStamp(string token, string expectedTokenType);
}
