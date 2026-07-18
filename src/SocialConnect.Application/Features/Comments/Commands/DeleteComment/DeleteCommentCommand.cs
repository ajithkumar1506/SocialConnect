using MediatR;
using SocialConnect.Application.Common.Models;

namespace SocialConnect.Application.Features.Comments.Commands.DeleteComment;

public record DeleteCommentCommand(Guid CommentId) : IRequest<Result<bool>>;
