using MediatR;
using SocialConnect.Application.Common.Models;

namespace SocialConnect.Application.Features.Comments.Commands.UpdateComment;

public record UpdateCommentCommand(Guid CommentId, string Content) : IRequest<Result<bool>>;
