using MediatR;
using Microsoft.EntityFrameworkCore;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Common.Models;

namespace SocialConnect.Application.Features.Comments.Commands.DeleteComment;

public class DeleteCommentCommandHandler : IRequestHandler<DeleteCommentCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DeleteCommentCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId == null || userId == Guid.Empty)
        {
            return Result<bool>.Failure("Unauthorized access.");
        }

        var comment = await _context.Comments
            .FirstOrDefaultAsync(c => c.Id == request.CommentId, cancellationToken);

        if (comment == null)
        {
            return Result<bool>.Failure("Comment not found.");
        }

        if (comment.AuthorId != userId.Value)
        {
            return Result<bool>.Failure("You are not authorized to delete this comment.");
        }

        // Remove the comment. The AuditableEntitySaveChangesInterceptor will intercept this
        // and perform a soft-delete (setting IsDeleted = true, DeletedAt = Now).
        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
