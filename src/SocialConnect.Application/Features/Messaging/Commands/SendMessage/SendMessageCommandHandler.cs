using MediatR;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Common.Models;
using SocialConnect.Domain.Entities.Messaging;
using SocialConnect.Domain.Repositories;

namespace SocialConnect.Application.Features.Messaging.Commands.SendMessage;

public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, Result<Guid>>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public SendMessageCommandHandler(
        IConversationRepository conversationRepository,
        IMessageRepository messageRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService
    )
    {
        _conversationRepository = conversationRepository;
        _messageRepository = messageRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(
        SendMessageCommand request,
        CancellationToken cancellationToken
    )
    {
        if (_currentUserService.UserId == null)
        {
            return Result<Guid>.Failure("User must be logged in to send a message.");
        }

        var conversation = await _conversationRepository.GetByIdWithMembersAsync(
            request.ConversationId,
            cancellationToken
        );
        if (conversation == null)
        {
            return Result<Guid>.Failure("Conversation not found.");
        }

        if (!conversation.Members.Any(m => m.UserId == _currentUserService.UserId.Value))
        {
            return Result<Guid>.Failure("You are not a member of this conversation.");
        }

        var message = Message.Create(
            conversation.Id,
            _currentUserService.UserId.Value,
            request.Content,
            request.Type
        );

        await _messageRepository.AddAsync(message, cancellationToken);

        conversation.SetLastMessageAt(DateTimeOffset.UtcNow);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(message.Id);
    }
}
