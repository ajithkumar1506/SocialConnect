using MediatR;
using SocialConnect.Application.Common.Models;

namespace SocialConnect.Application.Features.Posts.Commands.PublishPost;

public record PublishPostCommand(Guid PostId) : IRequest<Result<bool>>;
