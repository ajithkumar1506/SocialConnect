using MediatR;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Common.Models;
using SocialConnect.Domain.Repositories;

namespace SocialConnect.Application.Features.Notifications.Commands.MarkAllNotificationsRead;

public class MarkAllNotificationsReadCommandHandler
    : IRequestHandler<MarkAllNotificationsReadCommand, Result<bool>>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public MarkAllNotificationsReadCommandHandler(
        INotificationRepository notificationRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService
    )
    {
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<bool>> Handle(
        MarkAllNotificationsReadCommand request,
        CancellationToken cancellationToken
    )
    {
        if (_currentUserService.UserId == null)
        {
            return Result<bool>.Failure("User must be logged in.");
        }

        var userId = _currentUserService.UserId.Value;

        await _notificationRepository.MarkAllAsReadAsync(userId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
