using MediatR;
using Microsoft.EntityFrameworkCore;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Common.Models;
using SocialConnect.Domain.Enums;
using SocialConnect.Domain.Repositories;

namespace SocialConnect.Application.Features.Messaging.Commands.MarkAsRead;

public class MarkAsReadCommandHandler : IRequestHandler<MarkAsReadCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public MarkAsReadCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork
    )
    {
        _context = context;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(MarkAsReadCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            return Result<bool>.Failure("User must be logged in.");
        }

        var currentUserId = _currentUserService.UserId.Value;

        var member = await _context.ConversationMembers
            .FirstOrDefaultAsync(m => m.ConversationId == request.ConversationId && m.UserId == currentUserId, cancellationToken);

        if (member == null)
        {
            return Result<bool>.Failure("Conversation member not found.");
        }

        var now = DateTimeOffset.UtcNow;
        member.UpdateLastReadAt(now);

        var unreadMessages = await _context.Messages
            .Where(m => m.ConversationId == request.ConversationId && m.SenderId != currentUserId && m.Status != MessageStatus.Read)
            .ToListAsync(cancellationToken);

        foreach (var msg in unreadMessages)
        {
            msg.MarkAsRead();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
