using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Features.Posts.DTOs;
using SocialConnect.Domain.Enums;

namespace SocialConnect.Application.Features.Posts.Queries.GetFeed;

public class GetFeedQueryHandler : IRequestHandler<GetFeedQuery, List<PostDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetFeedQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<PostDto>> Handle(
        GetFeedQuery request,
        CancellationToken cancellationToken
    )
    {
        var query = _context
            .Posts.AsNoTracking()
            .Where(p => p.Status == PostStatus.Published && !p.IsDeleted);

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
