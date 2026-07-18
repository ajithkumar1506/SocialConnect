using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Common.Models;
using SocialConnect.Application.Features.Posts.DTOs;
using SocialConnect.Domain.Enums;

namespace SocialConnect.Application.Features.Posts.Queries.SearchPosts;

public class SearchPostsQueryHandler : IRequestHandler<SearchPostsQuery, PaginatedList<PostDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public SearchPostsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService, IMapper mapper)
    {
        _context = context;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<PaginatedList<PostDto>> Handle(SearchPostsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Posts
            .AsNoTracking()
            .Where(p => p.Status == PostStatus.Published);

        var currentUserId = _currentUserService.UserId;
        if (currentUserId.HasValue && currentUserId.Value != Guid.Empty)
        {
            var userId = currentUserId.Value;

            // Authors who blocked or are blocked by the current user
            var blockedByMeQuery = _context.Blocks
                .Where(b => b.BlockerId == userId)
                .Select(b => b.BlockedId);

            var blockingMeQuery = _context.Blocks
                .Where(b => b.BlockedId == userId)
                .Select(b => b.BlockerId);

            var blockedUserIds = await blockedByMeQuery
                .Union(blockingMeQuery)
                .ToListAsync(cancellationToken);

            // Authors muted by current user
            var mutedUserIds = await _context.Mutes
                .Where(m => m.MuterId == userId)
                .Select(m => m.MutedId)
                .ToListAsync(cancellationToken);

            var excludedUserIds = blockedUserIds.Concat(mutedUserIds).Distinct().ToList();

            if (excludedUserIds.Count > 0)
            {
                query = query.Where(p => !excludedUserIds.Contains(p.AuthorId));
            }
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim().ToLower();
            query = query.Where(p => p.Content != null && ((string)p.Content)!.ToLower().Contains(term));
        }

        query = query.OrderByDescending(p => p.PublishedAt).ThenByDescending(p => p.Id);

        var count = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectTo<PostDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new PaginatedList<PostDto>(items, count, request.PageNumber, request.PageSize);
    }
}
