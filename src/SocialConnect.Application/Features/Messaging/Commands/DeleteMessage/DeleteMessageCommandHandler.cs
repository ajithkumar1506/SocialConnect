using MediatR;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Common.Models;
using SocialConnect.Domain.Enums;
using SocialConnect.Domain.Repositories;

namespace SocialConnect.Application.Features.Messaging.Commands.DeleteMessage;

public class DeleteMessageCommandHandler : IRequestHandler<DeleteMessageCommand, Result<bool>>
{
    private readonly IMessageRepository _messageRepository;
    private readonly IConversationRepository _conversationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DeleteMessageCommandHandler(
        IMessageRepository messageRepository,
        IConversationRepository conversationRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService
    )
    {
        _messageRepository = messageRepository;
        _conversationRepository = conversationRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(DeleteMessageCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            return Result<bool>.Failure("User must be logged in.");
        }

        var currentUserId = _currentUserService.UserId.Value;

        var message = await _messageRepository.GetByIdAsync(request.MessageId, cancellationToken);
        if (message == null || message.IsDeleted)
        {
            return Result<bool>.Failure("Message not found.");
        }

        if (message.SenderId != currentUserId)
        {
            var conversation = await _conversationRepository.GetByIdWithMembersAsync(message.ConversationId, cancellationToken);
            if (conversation == null)
            {
                return Result<bool>.Failure("Conversation not found.");
            }

            var member = conversation.Members.FirstOrDefault(m => m.UserId == currentUserId);
            if (member == null || member.Role != ConversationMemberRole.Admin)
            {
                return Result<bool>.Failure("You do not have permission to delete this message.");
            }
        }

        message.IsDeleted = true;
        message.DeletedAt = DateTimeOffset.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
