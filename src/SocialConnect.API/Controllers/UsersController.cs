using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Common.Models;
using SocialConnect.Application.Features.Users.Commands.UpdateProfile;
using SocialConnect.Application.Features.Users.Commands.UploadCoverImage;
using SocialConnect.Application.Features.Users.Commands.UploadProfileImage;
using SocialConnect.Application.Features.Users.DTOs;
using SocialConnect.Application.Features.Users.Queries.GetUserById;
using SocialConnect.Application.Features.Users.Queries.GetUserProfile;
using SocialConnect.Application.Features.Users.Queries.SearchUsers;

namespace SocialConnect.API.Controllers;

[Authorize]
public class UsersController : ApiControllerBase
{
    private readonly ICurrentUserService _currentUserService;

    public UsersController(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<UserProfileDto>>> GetCurrentUser()
    {
        if (_currentUserService.UserId == null)
        {
            return BadRequest(ApiResponse<UserProfileDto>.Failure("User is not authenticated."));
        }

        var profile = await Mediator.Send(
            new GetUserProfileQuery(_currentUserService.UserId.Value)
        );
        if (profile == null)
        {
            return NotFound(
                ApiResponse<UserProfileDto>.Failure("Profile not found for current user.")
            );
        }

        return Ok(ApiResponse<UserProfileDto>.Ok(profile, "Profile retrieved successfully."));
    }

    [HttpPut("me")]
    public async Task<ActionResult<ApiResponse<UserProfileDto>>> UpdateProfile([FromBody] UpdateProfileCommand command)
    {
        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<UserProfileDto>.Failure(result.Error ?? "Could not update profile."));
        }

        return Ok(ApiResponse<UserProfileDto>.Ok(result.Value!, "Profile updated successfully."));
    }

    [HttpPost("me/profile-image")]
    public async Task<ActionResult<ApiResponse<string>>> UploadProfileImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(ApiResponse<string>.Failure("No file was uploaded."));
        }

        using var stream = file.OpenReadStream();
        var command = new UploadProfileImageCommand(stream, file.FileName, file.ContentType);
        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<string>.Failure(result.Error ?? "Could not upload profile image."));
        }

        return Ok(ApiResponse<string>.Ok(result.Value!, "Profile image uploaded successfully."));
    }

    [HttpPost("me/cover-image")]
    public async Task<ActionResult<ApiResponse<string>>> UploadCoverImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(ApiResponse<string>.Failure("No file was uploaded."));
        }

        using var stream = file.OpenReadStream();
        var command = new UploadCoverImageCommand(stream, file.FileName, file.ContentType);
        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<string>.Failure(result.Error ?? "Could not upload cover image."));
        }

        return Ok(ApiResponse<string>.Ok(result.Value!, "Cover image uploaded successfully."));
    }

    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<PaginatedList<UserProfileDto>>>> SearchUsers(
        [FromQuery] string searchTerm = "",
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10
    )
    {
        var query = new SearchUsersQuery(searchTerm, pageNumber, pageSize);
        var result = await Mediator.Send(query);
        return Ok(ApiResponse<PaginatedList<UserProfileDto>>.Ok(result, "Users searched successfully."));
    }

    [HttpGet("{userId:guid}")]
    public async Task<ActionResult<ApiResponse<UserProfileDto>>> GetUserProfile(Guid userId)
    {
        var profile = await Mediator.Send(new GetUserProfileQuery(userId));
        if (profile == null)
        {
            return NotFound(ApiResponse<UserProfileDto>.Failure("Profile not found."));
        }

        return Ok(ApiResponse<UserProfileDto>.Ok(profile, "Profile retrieved successfully."));
    }

    [HttpGet("{userId:guid}/details")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetUserById(Guid userId)
    {
        var user = await Mediator.Send(new GetUserByIdQuery(userId));
        if (user == null)
        {
            return NotFound(ApiResponse<UserDto>.Failure("User not found."));
        }

        return Ok(ApiResponse<UserDto>.Ok(user, "User retrieved successfully."));
    }
}
