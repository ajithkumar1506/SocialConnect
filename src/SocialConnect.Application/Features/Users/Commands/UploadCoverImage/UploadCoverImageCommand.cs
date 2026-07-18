using MediatR;
using SocialConnect.Application.Common.Models;

namespace SocialConnect.Application.Features.Users.Commands.UploadCoverImage;

public record UploadCoverImageCommand(
    Stream FileStream,
    string FileName,
    string ContentType
) : IRequest<Result<string>>;
