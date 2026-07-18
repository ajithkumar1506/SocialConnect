using MediatR;
using SocialConnect.Application.Common.Models;
using SocialConnect.Application.Features.Posts.DTOs;

namespace SocialConnect.Application.Features.Posts.Queries.SearchPosts;

public record SearchPostsQuery(
    string SearchTerm,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<PaginatedList<PostDto>>;
