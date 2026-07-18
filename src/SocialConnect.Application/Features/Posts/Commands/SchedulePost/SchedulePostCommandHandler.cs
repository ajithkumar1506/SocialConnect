using MediatR;
using Microsoft.EntityFrameworkCore;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Common.Models;

namespace SocialConnect.Application.Features.Posts.Commands.SchedulePost;

public class SchedulePostCommandHandler : IRequestHandler<SchedulePostCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public SchedulePostCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(SchedulePostCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId == null || userId == Guid.Empty)
        {
            return Result<bool>.Failure("Unauthorized access.");
        }

        var post = await _context.Posts
            .FirstOrDefaultAsync(p => p.Id == request.PostId, cancellationToken);

        if (post == null)
        {
            return Result<bool>.Failure("Post not found.");
        }

        if (post.AuthorId != userId.Value)
        {
            return Result<bool>.Failure("You are not authorized to schedule this post.");
        }

        try
        {
            post.Schedule(request.ScheduledAt);
            await _context.SaveChangesAsync(cancellationToken);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(ex.Message);
        }
    }
}
