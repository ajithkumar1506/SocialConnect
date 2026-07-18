using MediatR;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Common.Models;
using SocialConnect.Domain.Repositories;

namespace SocialConnect.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;
    private readonly IDateTime _dateTime;

    public RefreshTokenCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ITokenService tokenService,
        IDateTime dateTime
    )
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
        _dateTime = dateTime;
    }

    public async Task<AuthResult> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken
    )
    {
        var principal = _tokenService.GetPrincipalFromExpiredToken(request.Token);
        var email = principal
            .Claims.FirstOrDefault(c =>
                c.Type == "email" || c.Type == System.Security.Claims.ClaimTypes.Email
            )
            ?.Value;

        if (email == null)
            throw new UnauthorizedAccessException("Invalid token claims.");

        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
        if (user == null)
            throw new UnauthorizedAccessException("User not found.");

        var savedRefreshToken = user.RefreshTokens.FirstOrDefault(rt =>
            rt.TokenHash == request.RefreshToken
        );

        if (savedRefreshToken == null || savedRefreshToken.ExpiresAt <= _dateTime.Now)
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");

        // Remove old refresh token
        user.RemoveRefreshToken(savedRefreshToken);

        // Generate new tokens
        var newToken = _tokenService.GenerateAccessToken(
            user.Id,
            user.Email.Value,
            new List<string>()
        );
        var newRefreshToken = _tokenService.GenerateRefreshToken();
        var expiration = _dateTime.Now.AddDays(7);

        var rt = SocialConnect.Domain.Entities.Users.RefreshToken.Create(
            user.Id,
            newRefreshToken,
            Guid.NewGuid().ToString(),
            expiration,
            "Unknown",
            null
        );

        user.AddRefreshToken(rt);

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResult
        {
            Token = newToken,
            RefreshToken = newRefreshToken,
            RefreshTokenExpiration = expiration,
        };
    }
}
