using MediatR;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Common.Models;
using SocialConnect.Domain.Enums;
using SocialConnect.Domain.Repositories;

namespace SocialConnect.Application.Features.Messaging.Commands.RemoveGroupMember;

public class RemoveGroupMemberCommandHandler : IRequestHandler<RemoveGroupMemberCommand, Result<bool>>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public RemoveGroupMemberCommandHandler(
        IConversationRepository conversationRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService
    )
    {
        _conversationRepository = conversationRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(RemoveGroupMemberCommand request, CancellationToken cancellationToken)
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

        if (conversation.Type != ConversationType.Group)
        {
            return Result<bool>.Failure("Cannot remove members from a one-to-one conversation.");
        }

        var currentUserMember = conversation.Members.FirstOrDefault(m => m.UserId == currentUserId);
        if (currentUserMember == null)
        {
            return Result<bool>.Failure("You are not a member of this conversation.");
        }

        if (request.UserId != currentUserId && currentUserMember.Role != ConversationMemberRole.Admin)
        {
            return Result<bool>.Failure("Only admins can remove other members from the group.");
        }

        var memberToRemove = conversation.Members.FirstOrDefault(m => m.UserId == request.UserId);
        if (memberToRemove == null)
        {
            return Result<bool>.Failure("User is not a member of this conversation.");
        }

        conversation.RemoveMember(request.UserId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
