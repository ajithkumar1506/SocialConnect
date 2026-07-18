using MediatR;
using SocialConnect.Application.Features.Posts.DTOs;

namespace SocialConnect.Application.Features.Posts.Queries.GetFeed;

public record GetFeedQuery(int PageSize, DateTimeOffset? CursorScore, Guid? CursorId)
    : IRequest<List<PostDto>>;
