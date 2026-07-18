using MediatR;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Common.Models;
using SocialConnect.Domain.Entities.Messaging;
using SocialConnect.Domain.Repositories;

namespace SocialConnect.Application.Features.Messaging.Commands.CreateGroupChat;

public class CreateGroupChatCommandHandler : IRequestHandler<CreateGroupChatCommand, Result<Guid>>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreateGroupChatCommandHandler(
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

    public async Task<Result<Guid>> Handle(CreateGroupChatCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            return Result<Guid>.Failure("User must be logged in.");
        }

        var currentUserId = _currentUserService.UserId.Value;

        var conversation = Conversation.CreateGroup(request.Name, currentUserId, request.ImageUrl);

        if (request.MemberIds != null)
        {
            foreach (var memberId in request.MemberIds)
            {
                var user = await _userRepository.GetByIdAsync(memberId, cancellationToken);
                if (user != null)
                {
                    conversation.AddMember(memberId);
                }
            }
        }

        await _conversationRepository.AddAsync(conversation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(conversation.Id);
    }
}
