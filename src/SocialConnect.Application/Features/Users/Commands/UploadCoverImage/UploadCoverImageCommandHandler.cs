using MediatR;
using Microsoft.EntityFrameworkCore;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Common.Models;

namespace SocialConnect.Application.Features.Users.Commands.UploadCoverImage;

public class UploadCoverImageCommandHandler : IRequestHandler<UploadCoverImageCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorageService;

    private static readonly string[] AllowedContentTypes = { "image/jpeg", "image/png", "image/gif", "image/webp" };

    public UploadCoverImageCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IFileStorageService fileStorageService
    )
    {
        _context = context;
        _currentUserService = currentUserService;
        _fileStorageService = fileStorageService;
    }

    public async Task<Result<string>> Handle(
        UploadCoverImageCommand request,
        CancellationToken cancellationToken
    )
    {
        var userId = _currentUserService.UserId;
        if (userId == null || userId == Guid.Empty)
        {
            return Result<string>.Failure("Unauthorized access.");
        }

        if (!AllowedContentTypes.Contains(request.ContentType.ToLower()))
        {
            return Result<string>.Failure("Invalid file type. Only JPEG, PNG, GIF, and WebP images are allowed.");
        }

        var profile = await _context.UserProfiles
            .FirstOrDefaultAsync(up => up.UserId == userId.Value, cancellationToken);

        if (profile == null)
        {
            return Result<string>.Failure("User profile not found.");
        }

        // Delete old image if it exists
        if (!string.IsNullOrEmpty(profile.CoverImageUrl))
        {
            try
            {
                await _fileStorageService.DeleteFileAsync(profile.CoverImageUrl, cancellationToken);
            }
            catch
            {
                // Log and continue if deletion fails
            }
        }

        // Upload new image
        var imageUrl = await _fileStorageService.UploadFileAsync(
            request.FileStream,
            request.FileName,
            request.ContentType,
            cancellationToken
        );

        profile.UpdateCoverImage(imageUrl);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<string>.Success(imageUrl);
    }
}
