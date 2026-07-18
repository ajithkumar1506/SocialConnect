using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialConnect.Application.Common.Models;
using SocialConnect.Application.Features.Reactions.Commands.ToggleReaction;
using SocialConnect.Application.Features.Reactions.DTOs;
using SocialConnect.Application.Features.Reactions.Queries.GetReactions;
using SocialConnect.Domain.Enums;

namespace SocialConnect.API.Controllers;

[Authorize]
public class ReactionsController : ApiControllerBase
{
    [HttpPost("toggle")]
    public async Task<ActionResult<ApiResponse<bool>>> Toggle(
        [FromBody] ToggleReactionCommand command
    )
    {
        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(
                ApiResponse<bool>.Failure(result.Error ?? "Could not toggle reaction.")
            );
        }

        return Ok(ApiResponse<bool>.Ok(result.Value, "Reaction toggled successfully."));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedList<ReactionDto>>>> GetReactions(
        [FromQuery] Guid targetId,
        [FromQuery] TargetType targetType,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10
    )
    {
        var query = new GetReactionsQuery(targetId, targetType, pageNumber, pageSize);
        var result = await Mediator.Send(query);
        return Ok(ApiResponse<PaginatedList<ReactionDto>>.Ok(result, "Reactions retrieved successfully."));
    }
}
