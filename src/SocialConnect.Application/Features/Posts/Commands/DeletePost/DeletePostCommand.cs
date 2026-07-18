using MediatR;
using SocialConnect.Application.Common.Models;

namespace SocialConnect.Application.Features.Posts.Commands.DeletePost;

public record DeletePostCommand(Guid PostId) : IRequest<Result<bool>>;
