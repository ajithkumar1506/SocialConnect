using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialConnect.API.Models;
using SocialConnect.Application.Common.Models;
using SocialConnect.Application.Features.Posts.Commands.CreatePost;
using SocialConnect.Application.Features.Posts.Commands.DeletePost;
using SocialConnect.Application.Features.Posts.Commands.PublishPost;
using SocialConnect.Application.Features.Posts.Commands.SchedulePost;
using SocialConnect.Application.Features.Posts.Commands.UpdatePost;
using SocialConnect.Application.Features.Posts.Commands.UploadPostMedia;
using SocialConnect.Application.Features.Posts.DTOs;
using SocialConnect.Application.Features.Posts.Queries.GetFeed;
using SocialConnect.Application.Features.Posts.Queries.GetPostById;
using SocialConnect.Application.Features.Posts.Queries.GetUserPosts;
using SocialConnect.Application.Features.Posts.Queries.SearchPosts;

namespace SocialConnect.API.Controllers;

[Authorize]
public class PostsController : ApiControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ApiResponse<Guid>>> Create([FromBody] CreatePostCommand command)
    {
        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<Guid>.Failure(result.Error ?? "Could not create post."));
        }

        return Ok(ApiResponse<Guid>.Ok(result.Value, "Post created successfully."));
    }

    [HttpGet("feed")]
    public async Task<ActionResult<ApiResponse<List<PostDto>>>> GetFeed(
        [FromQuery] int pageSize = 10,
        [FromQuery] DateTimeOffset? cursorScore = null,
        [FromQuery] Guid? cursorId = null
    )
    {
        var query = new GetFeedQuery(pageSize, cursorScore, cursorId);
        var feed = await Mediator.Send(query);
        return Ok(ApiResponse<List<PostDto>>.Ok(feed, "Feed retrieved successfully."));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<PostDto>>> GetById(Guid id)
    {
        var query = new GetPostByIdQuery(id);
        var post = await Mediator.Send(query);

        if (post == null)
        {
            return NotFound(ApiResponse<PostDto>.Failure("Post not found."));
        }

        return Ok(ApiResponse<PostDto>.Ok(post, "Post retrieved successfully."));
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<ApiResponse<List<PostDto>>>> GetUserPosts(
        Guid userId,
        [FromQuery] int pageSize = 10,
        [FromQuery] DateTimeOffset? cursorScore = null,
        [FromQuery] Guid? cursorId = null
    )
    {
        var query = new GetUserPostsQuery(userId, pageSize, cursorScore, cursorId);
        var posts = await Mediator.Send(query);
        return Ok(ApiResponse<List<PostDto>>.Ok(posts, "User posts retrieved successfully."));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<Guid>>> Update(
        Guid id,
        [FromBody] UpdatePostRequest request
    )
    {
        var command = new UpdatePostCommand(id, request.Content);
        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<Guid>.Failure(result.Error ?? "Could not update post."));
        }

        return Ok(ApiResponse<Guid>.Ok(result.Value, "Post updated successfully."));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        var command = new DeletePostCommand(id);
        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<bool>.Failure(result.Error ?? "Could not delete post."));
        }

        return Ok(ApiResponse<bool>.Ok(result.Value, "Post deleted successfully."));
    }

    [HttpPost("{id:guid}/publish")]
    public async Task<ActionResult<ApiResponse<bool>>> Publish(Guid id)
    {
        var command = new PublishPostCommand(id);
        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<bool>.Failure(result.Error ?? "Could not publish post."));
        }

        return Ok(ApiResponse<bool>.Ok(result.Value, "Post published successfully."));
    }

    [HttpPost("{id:guid}/schedule")]
    public async Task<ActionResult<ApiResponse<bool>>> Schedule(Guid id, [FromBody] DateTimeOffset scheduledAt)
    {
        var command = new SchedulePostCommand(id, scheduledAt);
        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<bool>.Failure(result.Error ?? "Could not schedule post."));
        }

        return Ok(ApiResponse<bool>.Ok(result.Value, "Post scheduled successfully."));
    }

    [HttpPost("media")]
    public async Task<ActionResult<ApiResponse<MediaItemDto>>> UploadMedia(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(ApiResponse<MediaItemDto>.Failure("No file was uploaded."));
        }

        using var stream = file.OpenReadStream();
        var command = new UploadPostMediaCommand(stream, file.FileName, file.ContentType);
        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<MediaItemDto>.Failure(result.Error ?? "Could not upload media."));
        }

        return Ok(ApiResponse<MediaItemDto>.Ok(result.Value!, "Post media uploaded successfully."));
    }

    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<PaginatedList<PostDto>>>> Search(
        [FromQuery] string searchTerm = "",
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10
    )
    {
        var query = new SearchPostsQuery(searchTerm, pageNumber, pageSize);
        var result = await Mediator.Send(query);
        return Ok(ApiResponse<PaginatedList<PostDto>>.Ok(result, "Posts searched successfully."));
    }
}
