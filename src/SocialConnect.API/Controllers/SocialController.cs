using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialConnect.Application.Common.Models;
using SocialConnect.Application.Features.Social.Commands.BlockUser;
using SocialConnect.Application.Features.Social.Commands.FollowUser;
using SocialConnect.Application.Features.Social.Commands.MuteUser;
using SocialConnect.Application.Features.Social.Commands.UnblockUser;
using SocialConnect.Application.Features.Social.Commands.UnfollowUser;
using SocialConnect.Application.Features.Social.Commands.UnmuteUser;
using SocialConnect.Application.Features.Social.DTOs;
using SocialConnect.Application.Features.Social.Queries.GetFollowers;
using SocialConnect.Application.Features.Social.Queries.GetFollowing;

namespace SocialConnect.API.Controllers;

[Authorize]
public class SocialController : ApiControllerBase
{
    [HttpPost("follow/{userId}")]
    public async Task<ActionResult<ApiResponse<Guid>>> Follow(Guid userId)
    {
        var command = new FollowUserCommand(userId);
        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<Guid>.Failure(result.Error ?? "Could not follow user."));
        }

        return Ok(ApiResponse<Guid>.Ok(result.Value, "User followed successfully."));
    }

    [HttpPost("unfollow/{userId}")]
    public async Task<ActionResult<ApiResponse<Guid>>> Unfollow(Guid userId)
    {
        var command = new UnfollowUserCommand(userId);
        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(
                ApiResponse<Guid>.Failure(result.Error ?? "Could not unfollow user.")
            );
        }

        return Ok(ApiResponse<Guid>.Ok(result.Value, "User unfollowed successfully."));
    }

    [HttpPost("block/{userId:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> Block(Guid userId)
    {
        var command = new BlockUserCommand(userId);
        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<bool>.Failure(result.Error ?? "Could not block user."));
        }

        return Ok(ApiResponse<bool>.Ok(result.Value, "User blocked successfully."));
    }

    [HttpPost("unblock/{userId:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> Unblock(Guid userId)
    {
        var command = new UnblockUserCommand(userId);
        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<bool>.Failure(result.Error ?? "Could not unblock user."));
        }

        return Ok(ApiResponse<bool>.Ok(result.Value, "User unblocked successfully."));
    }

    [HttpPost("mute/{userId:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> Mute(Guid userId)
    {
        var command = new MuteUserCommand(userId);
        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<bool>.Failure(result.Error ?? "Could not mute user."));
        }

        return Ok(ApiResponse<bool>.Ok(result.Value, "User muted successfully."));
    }

    [HttpPost("unmute/{userId:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> Unmute(Guid userId)
    {
        var command = new UnmuteUserCommand(userId);
        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<bool>.Failure(result.Error ?? "Could not unmute user."));
        }

        return Ok(ApiResponse<bool>.Ok(result.Value, "User unmuted successfully."));
    }

    [HttpGet("followers/{userId}")]
    public async Task<ActionResult<ApiResponse<List<FollowUserDto>>>> GetFollowers(
        Guid userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10
    )
    {
        var query = new GetFollowersQuery(userId, page, pageSize);
        var followers = await Mediator.Send(query);
        return Ok(
            ApiResponse<List<FollowUserDto>>.Ok(followers, "Followers retrieved successfully.")
        );
    }

    [HttpGet("following/{userId}")]
    public async Task<ActionResult<ApiResponse<List<FollowUserDto>>>> GetFollowing(
        Guid userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10
    )
    {
        var query = new GetFollowingQuery(userId, page, pageSize);
        var following = await Mediator.Send(query);
        return Ok(
            ApiResponse<List<FollowUserDto>>.Ok(following, "Following retrieved successfully.")
        );
    }
}
