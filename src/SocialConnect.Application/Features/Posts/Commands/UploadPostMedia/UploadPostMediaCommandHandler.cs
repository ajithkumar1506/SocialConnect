using MediatR;
using SocialConnect.Application.Common.Helpers;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Common.Models;
using SocialConnect.Application.Features.Posts.DTOs;
using SocialConnect.Domain.Enums;

namespace SocialConnect.Application.Features.Posts.Commands.UploadPostMedia;

public class UploadPostMediaCommandHandler : IRequestHandler<UploadPostMediaCommand, Result<MediaItemDto>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorageService;

    public UploadPostMediaCommandHandler(ICurrentUserService currentUserService, IFileStorageService fileStorageService)
    {
        _currentUserService = currentUserService;
        _fileStorageService = fileStorageService;
    }

    public async Task<Result<MediaItemDto>> Handle(UploadPostMediaCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId == null || userId == Guid.Empty)
        {
            return Result<MediaItemDto>.Failure("Unauthorized access.");
        }

        var contentType = request.ContentType.ToLowerInvariant();
        MediaType mediaType;
        if (contentType.StartsWith("image/"))
        {
            mediaType = MediaType.Image;

            // Perform image dimension checks
            var dimensions = ImageMetadataReader.GetDimensions(request.FileStream, request.ContentType);

            // Reset position so S3 client can read the stream from the start
            request.FileStream.Position = 0;

            if (dimensions.HasValue)
            {
                var (width, height) = dimensions.Value;
                if (width > 4096 || height > 4096)
                {
                    return Result<MediaItemDto>.Failure("Image dimensions exceed the maximum allowed size (4096x4096px).");
                }
            }
        }
        else if (contentType.StartsWith("video/"))
        {
            mediaType = MediaType.Video;
        }
        else
        {
            return Result<MediaItemDto>.Failure("Unsupported media type. Only images and videos are supported.");
        }

        var fileSize = request.FileStream.Length;

        var url = await _fileStorageService.UploadFileAsync(
            request.FileStream,
            request.FileName,
            request.ContentType,
            cancellationToken
        );

        var dto = new MediaItemDto(url, mediaType, fileSize);
        return Result<MediaItemDto>.Success(dto);
    }
}
