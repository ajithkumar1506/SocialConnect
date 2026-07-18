using MediatR;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Common.Models;
using SocialConnect.Domain.Repositories;

namespace SocialConnect.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IDateTime _dateTime;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ITokenService tokenService,
        IPasswordHasher passwordHasher,
        IDateTime dateTime
    )
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
        _dateTime = dateTime;
    }

    public async Task<AuthResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (
            user == null
            || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash.Hash)
        )
        {
            throw new UnauthorizedAccessException();
        }

        var token = _tokenService.GenerateAccessToken(
            user.Id,
            user.Email.Value,
            new List<string>()
        );
        var refreshToken = _tokenService.GenerateRefreshToken();
        var expiration = _dateTime.Now.AddDays(7);

        var rt = SocialConnect.Domain.Entities.Users.RefreshToken.Create(
            user.Id,
            refreshToken,
            Guid.NewGuid().ToString(),
            expiration,
            "Unknown",
            null
        );

        user.AddRefreshToken(rt);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResult
        {
            Token = token,
            RefreshToken = refreshToken,
            RefreshTokenExpiration = expiration,
        };
    }
}
