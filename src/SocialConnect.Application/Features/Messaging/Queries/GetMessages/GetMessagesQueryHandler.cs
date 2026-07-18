using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Common.Models;
using SocialConnect.Application.Features.Messaging.DTOs;

namespace SocialConnect.Application.Features.Messaging.Queries.GetMessages;

public class GetMessagesQueryHandler : IRequestHandler<GetMessagesQuery, PaginatedList<MessageDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetMessagesQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IMapper mapper
    )
    {
        _context = context;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<PaginatedList<MessageDto>> Handle(GetMessagesQuery request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            throw new UnauthorizedAccessException("User must be logged in.");
        }

        var currentUserId = _currentUserService.UserId.Value;

        var isMember = await _context.ConversationMembers
            .AnyAsync(m => m.ConversationId == request.ConversationId && m.UserId == currentUserId, cancellationToken);

        if (!isMember)
        {
            throw new UnauthorizedAccessException("You are not a member of this conversation.");
        }

        var baseQuery = _context.Messages
            .AsNoTracking()
            .Where(m => m.ConversationId == request.ConversationId && !m.IsDeleted);

        var totalCount = await baseQuery.CountAsync(cancellationToken);

        var items = await baseQuery
            .OrderByDescending(m => m.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Include("Sender.Profile")
            .Include(m => m.Attachments)
            .ProjectTo<MessageDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        items.Reverse();

        return new PaginatedList<MessageDto>(items, totalCount, request.PageNumber, request.PageSize);
    }
}
