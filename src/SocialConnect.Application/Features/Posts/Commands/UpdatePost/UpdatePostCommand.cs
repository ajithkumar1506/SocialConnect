using MediatR;
using SocialConnect.Application.Common.Models;

namespace SocialConnect.Application.Features.Posts.Commands.UpdatePost;

public record UpdatePostCommand(Guid PostId, string Content) : IRequest<Result<Guid>>;
