using MediatR;
using SocialConnect.Application.Features.Comments.DTOs;
using SocialConnect.Domain.Repositories;

namespace SocialConnect.Application.Features.Comments.Queries.GetPostComments;

public class GetPostCommentsQueryHandler : IRequestHandler<GetPostCommentsQuery, List<CommentDto>>
{
    private readonly ICommentRepository _commentRepository;

    public GetPostCommentsQueryHandler(ICommentRepository commentRepository)
    {
        _commentRepository = commentRepository;
    }

    public async Task<List<CommentDto>> Handle(
        GetPostCommentsQuery request,
        CancellationToken cancellationToken
    )
    {
        var comments = await _commentRepository.GetByPostIdAsync(
            request.PostId,
            request.Page,
            request.PageSize,
            cancellationToken
        );

        return comments
            .Select(c => new CommentDto
            {
                Id = c.Id,
                PostId = c.PostId,
                AuthorId = c.AuthorId,
                AuthorName =
                    c.Author != null
                        ? $"{c.Author.Profile?.FirstName} {c.Author.Profile?.LastName}".Trim()
                        : "Unknown User",
                AuthorProfileImageUrl = c.Author?.Profile?.ProfileImageUrl,
                Content = c.Content,
                ParentCommentId = c.ParentCommentId,
                Depth = c.Depth,
                ReactionCount = c.ReactionCount,
                CreatedAt = c.CreatedAt,
            })
            .ToList();
    }
}
