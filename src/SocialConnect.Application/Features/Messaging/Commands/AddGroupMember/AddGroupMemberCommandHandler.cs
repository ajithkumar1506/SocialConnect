using MediatR;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Common.Models;
using SocialConnect.Domain.Repositories;

namespace SocialConnect.Application.Features.Messaging.Commands.AddGroupMember;

public class AddGroupMemberCommandHandler : IRequestHandler<AddGroupMemberCommand, Result<bool>>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public AddGroupMemberCommandHandler(
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

    public async Task<Result<bool>> Handle(AddGroupMemberCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            return Result<bool>.Failure("User must be logged in.");
        }

        var currentUserId = _currentUserService.UserId.Value;

        var conversation = await _conversationRepository.GetByIdWithMembersAsync(request.ConversationId, cancellationToken);
        if (conversation == null)
        {
            return Result<bool>.Failure("Conversation not found.");
        }

        if (conversation.Type != Domain.Enums.ConversationType.Group)
        {
            return Result<bool>.Failure("Cannot add members to a one-to-one conversation.");
        }

        var isCurrentUserMember = conversation.Members.Any(m => m.UserId == currentUserId);
        if (!isCurrentUserMember)
        {
            return Result<bool>.Failure("You must be a member of the group to add others.");
        }

        var userToAdd = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (userToAdd == null)
        {
            return Result<bool>.Failure("User to add not found.");
        }

        conversation.AddMember(request.UserId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
