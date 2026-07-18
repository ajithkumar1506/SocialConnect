using MediatR;
using SocialConnect.Application.Features.Posts.DTOs;

namespace SocialConnect.Application.Features.Posts.Queries.GetUserPosts;

public record GetUserPostsQuery(
    Guid UserId,
    int PageSize = 10,
    DateTimeOffset? CursorScore = null,
    Guid? CursorId = null
) : IRequest<List<PostDto>>;
