using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Common.Models;
using SocialConnect.Application.Features.Messaging.DTOs;

namespace SocialConnect.Application.Features.Messaging.Queries.GetConversations;

public class GetConversationsQueryHandler : IRequestHandler<GetConversationsQuery, PaginatedList<ConversationDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetConversationsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IMapper mapper
    )
    {
        _context = context;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<PaginatedList<ConversationDto>> Handle(GetConversationsQuery request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            return new PaginatedList<ConversationDto>(new List<ConversationDto>(), 0, request.PageNumber, request.PageSize);
        }

        var currentUserId = _currentUserService.UserId.Value;

        var baseQuery = _context.Conversations
            .AsNoTracking()
            .Where(c => c.Members.Any(m => m.UserId == currentUserId));

        var totalCount = await baseQuery.CountAsync(cancellationToken);

        var conversations = await baseQuery
            .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Include("Members.User.Profile")
            .ToListAsync(cancellationToken);

        var dtoList = new List<ConversationDto>();

        foreach (var conv in conversations)
        {
            var dto = _mapper.Map<ConversationDto>(conv);

            var lastMessage = await _context.Messages
                .AsNoTracking()
                .Where(m => m.ConversationId == conv.Id && !m.IsDeleted)
                .OrderByDescending(m => m.CreatedAt)
                .Include("Sender.Profile")
                .Include(m => m.Attachments)
                .FirstOrDefaultAsync(cancellationToken);

            if (lastMessage != null)
            {
                dto.LastMessage = _mapper.Map<MessageDto>(lastMessage);
            }

            var myMember = conv.Members.FirstOrDefault(m => m.UserId == currentUserId);
            if (myMember != null)
            {
                var lastReadAt = myMember.LastReadAt;
                dto.UnreadCount = await _context.Messages
                    .Where(m => m.ConversationId == conv.Id && m.SenderId != currentUserId && !m.IsDeleted)
                    .Where(m => lastReadAt == null || m.CreatedAt > lastReadAt)
                    .CountAsync(cancellationToken);
            }

            dtoList.Add(dto);
        }

        return new PaginatedList<ConversationDto>(dtoList, totalCount, request.PageNumber, request.PageSize);
    }
}
