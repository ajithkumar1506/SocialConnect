using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Common.Models;
using SocialConnect.Application.Features.Reactions.DTOs;

namespace SocialConnect.Application.Features.Reactions.Queries.GetReactions;

public class GetReactionsQueryHandler : IRequestHandler<GetReactionsQuery, PaginatedList<ReactionDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetReactionsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedList<ReactionDto>> Handle(GetReactionsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Reactions
            .Include("User.Profile")
            .AsNoTracking()
            .Where(r => r.TargetId == request.TargetId && r.TargetType == request.TargetType);

        query = query.OrderByDescending(r => r.CreatedAt);

        var count = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectTo<ReactionDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new PaginatedList<ReactionDto>(items, count, request.PageNumber, request.PageSize);
    }
}
