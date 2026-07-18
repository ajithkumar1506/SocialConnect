using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialConnect.Application.Common.Models;
using SocialConnect.Application.Features.Comments.Commands.AddComment;
using SocialConnect.Application.Features.Comments.Commands.DeleteComment;
using SocialConnect.Application.Features.Comments.Commands.UpdateComment;
using SocialConnect.Application.Features.Comments.DTOs;
using SocialConnect.Application.Features.Comments.Queries.GetPostComments;

namespace SocialConnect.API.Controllers;

[Authorize]
public class CommentsController : ApiControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ApiResponse<Guid>>> Create([FromBody] AddCommentCommand command)
    {
        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<Guid>.Failure(result.Error ?? "Could not add comment."));
        }

        return Ok(ApiResponse<Guid>.Ok(result.Value, "Comment added successfully."));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> Update(Guid id, [FromBody] string content)
    {
        var command = new UpdateCommentCommand(id, content);
        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<bool>.Failure(result.Error ?? "Could not update comment."));
        }

        return Ok(ApiResponse<bool>.Ok(result.Value, "Comment updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        var command = new DeleteCommentCommand(id);
        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<bool>.Failure(result.Error ?? "Could not delete comment."));
        }

        return Ok(ApiResponse<bool>.Ok(result.Value, "Comment deleted successfully."));
    }

    [HttpGet("post/{postId}")]
    public async Task<ActionResult<ApiResponse<List<CommentDto>>>> GetPostComments(
        Guid postId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10
    )
    {
        var query = new GetPostCommentsQuery(postId, page, pageSize);
        var comments = await Mediator.Send(query);
        return Ok(ApiResponse<List<CommentDto>>.Ok(comments, "Comments retrieved successfully."));
    }
}
