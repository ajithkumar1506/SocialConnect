using MediatR;
using SocialConnect.Application.Common.Models;

namespace SocialConnect.Application.Features.Comments.Commands.AddComment;

public record AddCommentCommand(Guid PostId, string Content, Guid? ParentCommentId = null)
    : IRequest<Result<Guid>>;
