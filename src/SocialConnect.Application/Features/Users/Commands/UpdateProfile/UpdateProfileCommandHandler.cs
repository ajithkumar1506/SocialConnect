using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Common.Models;
using SocialConnect.Application.Features.Users.DTOs;

namespace SocialConnect.Application.Features.Users.Commands.UpdateProfile;

public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, Result<UserProfileDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public UpdateProfileCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IMapper mapper
    )
    {
        _context = context;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<Result<UserProfileDto>> Handle(
        UpdateProfileCommand request,
        CancellationToken cancellationToken
    )
    {
        var userId = _currentUserService.UserId;
        if (userId == null || userId == Guid.Empty)
        {
            return Result<UserProfileDto>.Failure("Unauthorized access.");
        }

        var profile = await _context.UserProfiles
            .FirstOrDefaultAsync(up => up.UserId == userId.Value, cancellationToken);

        if (profile == null)
        {
            return Result<UserProfileDto>.Failure("User profile not found.");
        }

        profile.UpdateDetails(
            request.FirstName,
            request.LastName,
            request.Headline,
            request.Bio,
            request.Location,
            request.DateOfBirth
        );

        await _context.SaveChangesAsync(cancellationToken);

        var dto = _mapper.Map<UserProfileDto>(profile);
        return Result<UserProfileDto>.Success(dto);
    }
}
