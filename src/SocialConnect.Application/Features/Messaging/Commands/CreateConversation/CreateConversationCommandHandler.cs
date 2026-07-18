using MediatR;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Common.Models;
using SocialConnect.Domain.Entities.Messaging;
using SocialConnect.Domain.Repositories;

namespace SocialConnect.Application.Features.Messaging.Commands.CreateConversation;

public class CreateConversationCommandHandler : IRequestHandler<CreateConversationCommand, Result<Guid>>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreateConversationCommandHandler(
        IConversationRepository conversationRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService
    )
    {
        _conversationRepository = conversationRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(CreateConversationCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            return Result<Guid>.Failure("User must be logged in.");
        }

        var currentUserId = _currentUserService.UserId.Value;

        if (currentUserId == request.RecipientId)
        {
            return Result<Guid>.Failure("Cannot create a conversation with yourself.");
        }

        var recipient = await _userRepository.GetByIdAsync(request.RecipientId, cancellationToken);
        if (recipient == null)
        {
            return Result<Guid>.Failure("Recipient user not found.");
        }

        var existing = await _conversationRepository.GetOneToOneConversationAsync(currentUserId, request.RecipientId, cancellationToken);
        if (existing != null)
        {
            return Result<Guid>.Success(existing.Id);
        }

        var conversation = Conversation.CreateOneToOne(currentUserId, request.RecipientId);
        await _conversationRepository.AddAsync(conversation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(conversation.Id);
    }
}
