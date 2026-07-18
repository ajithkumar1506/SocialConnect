using MediatR;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Common.Models;
using SocialConnect.Domain.Repositories;

namespace SocialConnect.Application.Features.Messaging.Commands.EditMessage;

public class EditMessageCommandHandler : IRequestHandler<EditMessageCommand, Result<bool>>
{
    private readonly IMessageRepository _messageRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public EditMessageCommandHandler(
        IMessageRepository messageRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService
    )
    {
        _messageRepository = messageRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(EditMessageCommand request, CancellationToken cancellationToken)
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
            return Result<bool>.Failure("You can only edit your own messages.");
        }

        message.Edit(request.Content);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
