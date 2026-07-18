using MediatR;
using SocialConnect.Application.Common.Models;
using SocialConnect.Application.Features.Posts.DTOs;

namespace SocialConnect.Application.Features.Posts.Commands.UploadPostMedia;

public record UploadPostMediaCommand(
    Stream FileStream,
    string FileName,
    string ContentType
) : IRequest<Result<MediaItemDto>>;
