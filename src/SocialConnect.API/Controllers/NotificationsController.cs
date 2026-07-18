using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialConnect.Application.Common.Models;
using SocialConnect.Application.Features.Notifications.Commands.MarkAllNotificationsRead;
using SocialConnect.Application.Features.Notifications.DTOs;
using SocialConnect.Application.Features.Notifications.Queries.GetUserNotifications;

namespace SocialConnect.API.Controllers;

[Authorize]
public class NotificationsController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<NotificationDto>>>> GetNotifications(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10
    )
    {
        var query = new GetUserNotificationsQuery(page, pageSize);
        var notifications = await Mediator.Send(query);
        return Ok(
            ApiResponse<List<NotificationDto>>.Ok(
                notifications,
                "Notifications retrieved successfully."
            )
        );
    }

    [HttpPost("read-all")]
    public async Task<ActionResult<ApiResponse<bool>>> ReadAll()
    {
        var command = new MarkAllNotificationsReadCommand();
        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(
                ApiResponse<bool>.Failure(result.Error ?? "Could not mark notifications as read.")
            );
        }

        return Ok(ApiResponse<bool>.Ok(result.Value, "All notifications marked as read."));
    }
}
