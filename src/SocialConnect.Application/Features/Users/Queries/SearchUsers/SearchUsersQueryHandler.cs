using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Common.Models;
using SocialConnect.Application.Features.Users.DTOs;

namespace SocialConnect.Application.Features.Users.Queries.SearchUsers;

public class SearchUsersQueryHandler : IRequestHandler<SearchUsersQuery, PaginatedList<UserProfileDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public SearchUsersQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedList<UserProfileDto>> Handle(
        SearchUsersQuery request,
        CancellationToken cancellationToken
    )
    {
        var query = _context.UserProfiles
            .Include(up => up.User)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim().ToLower();
            query = query.Where(up =>
                up.FirstName.ToLower().Contains(term)
                || up.LastName.ToLower().Contains(term)
                || (up.Headline != null && up.Headline.ToLower().Contains(term))
                || (up.User != null && up.User.UserName.ToLower().Contains(term))
                || (up.User != null && up.User.NormalizedEmail.ToLower().Contains(term))
            );
        }

        query = query.OrderBy(up => up.FirstName)
            .ThenBy(up => up.LastName)
            .ThenBy(up => up.Id);

        var count = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectTo<UserProfileDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new PaginatedList<UserProfileDto>(items, count, request.PageNumber, request.PageSize);
    }
}
