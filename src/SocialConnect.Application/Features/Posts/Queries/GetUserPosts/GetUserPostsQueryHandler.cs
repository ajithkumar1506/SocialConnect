using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Features.Posts.DTOs;
using SocialConnect.Domain.Enums;

namespace SocialConnect.Application.Features.Posts.Queries.GetUserPosts;

public class GetUserPostsQueryHandler : IRequestHandler<GetUserPostsQuery, List<PostDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetUserPostsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<PostDto>> Handle(
        GetUserPostsQuery request,
        CancellationToken cancellationToken
    )
    {
        var query = _context
            .Posts.AsNoTracking()
            .Where(p =>
                p.AuthorId == request.UserId && p.Status == PostStatus.Published && !p.IsDeleted
            );

        if (request.CursorScore.HasValue && request.CursorId.HasValue)
        {
            query = query.Where(p =>
                p.PublishedAt < request.CursorScore
                || (
                    p.PublishedAt == request.CursorScore
                    && p.Id.CompareTo(request.CursorId.Value) < 0
                )
            );
        }

        return await query
            .OrderByDescending(p => p.PublishedAt)
            .ThenByDescending(p => p.Id)
            .Take(request.PageSize)
            .ProjectTo<PostDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
