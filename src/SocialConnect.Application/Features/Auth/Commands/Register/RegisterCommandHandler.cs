using MediatR;
using SocialConnect.Application.Common.Exceptions;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Common.Models;
using SocialConnect.Domain.Entities.Users;
using SocialConnect.Domain.Repositories;
using SocialConnect.Domain.ValueObjects;

namespace SocialConnect.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IDateTime _dateTime;

    public RegisterCommandHandler(
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

    public async Task<AuthResult> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken
    )
    {
        // Check if email already exists
        var existingUserByEmail = await _userRepository.GetByEmailAsync(
            request.Email,
            cancellationToken
        );
        if (existingUserByEmail != null)
        {
            throw new ValidationException(
                new List<FluentValidation.Results.ValidationFailure>
                {
                    new("Email", "Email is already in use."),
                }
            );
        }

        // Check if username already exists
        var existingUserByUsername = await _userRepository.GetByUserNameAsync(
            request.Username,
            cancellationToken
        );
        if (existingUserByUsername != null)
        {
            throw new ValidationException(
                new List<FluentValidation.Results.ValidationFailure>
                {
                    new("Username", "Username is already taken."),
                }
            );
        }

        // Hash password
        var hashedPassword = _passwordHasher.HashPassword(request.Password);

        // Create user
        var email = Email.Create(request.Email);
        var password = Password.Create(hashedPassword);

        var user = User.Create(email, request.Username, password);
        user.CreatedAt = _dateTime.Now;

        // Initialize profile
        var profile = UserProfile.Create(user.Id, request.FirstName, request.LastName);
        profile.CreatedAt = _dateTime.Now;
        user.SetProfile(profile);

        // Add to repository
        await _userRepository.AddAsync(user, cancellationToken);

        // Generate Refresh Token
        var refreshToken = _tokenService.GenerateRefreshToken();
        var expiration = _dateTime.Now.AddDays(7); // Configurable

        var rt = SocialConnect.Domain.Entities.Users.RefreshToken.Create(
            user.Id,
            refreshToken, // using raw token as hash for now or you'd hash it
            Guid.NewGuid().ToString(), // JwtId
            expiration,
            "Unknown", // IP
            null // Device Info
        );

        user.AddRefreshToken(rt);

        // Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Generate JWT
        // In a real app, you would fetch roles from the user. For now, empty list.
        var token = _tokenService.GenerateAccessToken(
            user.Id,
            user.Email.Value,
            new List<string>()
        );

        return new AuthResult
        {
            Token = token,
            RefreshToken = refreshToken,
            RefreshTokenExpiration = expiration,
        };
    }
}
