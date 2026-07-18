using MediatR;
using SocialConnect.Application.Common.Models;

namespace SocialConnect.Application.Features.Users.Commands.UploadProfileImage;

public record UploadProfileImageCommand(
    Stream FileStream,
    string FileName,
    string ContentType
) : IRequest<Result<string>>;
