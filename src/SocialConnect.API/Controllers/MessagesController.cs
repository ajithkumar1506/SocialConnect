using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SocialConnect.API.Hubs;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Common.Models;
using SocialConnect.Application.Features.Messaging.Commands.AddGroupMember;
using SocialConnect.Application.Features.Messaging.Commands.CreateConversation;
using SocialConnect.Application.Features.Messaging.Commands.CreateGroupChat;
using SocialConnect.Application.Features.Messaging.Commands.DeleteMessage;
using SocialConnect.Application.Features.Messaging.Commands.EditMessage;
using SocialConnect.Application.Features.Messaging.Commands.MarkAsRead;
using SocialConnect.Application.Features.Messaging.Commands.RemoveGroupMember;
using SocialConnect.Application.Features.Messaging.Commands.SendMessage;
using SocialConnect.Application.Features.Messaging.DTOs;
using SocialConnect.Application.Features.Messaging.Queries.GetConversations;
using SocialConnect.Application.Features.Messaging.Queries.GetMessages;

namespace SocialConnect.API.Controllers;

[Authorize]
public class MessagesController : ApiControllerBase
{
    private readonly IHubContext<ChatHub> _chatHubContext;
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public MessagesController(
        IHubContext<ChatHub> chatHubContext,
        IApplicationDbContext context,
        IMapper mapper)
    {
        _chatHubContext = chatHubContext;
        _context = context;
        _mapper = mapper;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<Guid>>> SendMessage(
        [FromBody] SendMessageCommand command
    )
    {
        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<Guid>.Failure(result.Error ?? "Could not send message."));
        }

        var message = await _context.Messages
            .AsNoTracking()
            .Include("Sender.Profile")
            .Include(m => m.Attachments)
            .FirstOrDefaultAsync(m => m.Id == result.Value);

        if (message != null)
        {
            var messageDto = _mapper.Map<MessageDto>(message);
            await _chatHubContext.Clients.Group(message.ConversationId.ToString())
                .SendAsync("ReceiveMessage", messageDto);
        }

        return Ok(ApiResponse<Guid>.Ok(result.Value, "Message sent successfully."));
    }

    [HttpPost("conversation")]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateConversation(
        [FromBody] CreateConversationCommand command
    )
    {
        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<Guid>.Failure(result.Error ?? "Could not create conversation."));
        }

        return Ok(ApiResponse<Guid>.Ok(result.Value, "Conversation created successfully."));
    }

    [HttpPost("group")]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateGroupChat(
        [FromBody] CreateGroupChatCommand command
    )
    {
        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<Guid>.Failure(result.Error ?? "Could not create group chat."));
        }

        return Ok(ApiResponse<Guid>.Ok(result.Value, "Group chat created successfully."));
    }

    [HttpPost("conversation/{conversationId:guid}/members")]
    public async Task<ActionResult<ApiResponse<bool>>> AddGroupMember(
        Guid conversationId,
        [FromBody] Guid userId
    )
    {
        var command = new AddGroupMemberCommand(conversationId, userId);
        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<bool>.Failure(result.Error ?? "Could not add group member."));
        }

        return Ok(ApiResponse<bool>.Ok(result.Value, "Group member added successfully."));
    }

    [HttpDelete("conversation/{conversationId:guid}/members/{userId:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> RemoveGroupMember(
        Guid conversationId,
        Guid userId
    )
    {
        var command = new RemoveGroupMemberCommand(conversationId, userId);
        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<bool>.Failure(result.Error ?? "Could not remove group member."));
        }

        return Ok(ApiResponse<bool>.Ok(result.Value, "Group member removed successfully."));
    }

    [HttpPut("{messageId:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> EditMessage(
        Guid messageId,
        [FromBody] string content
    )
    {
        var command = new EditMessageCommand(messageId, content);
        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<bool>.Failure(result.Error ?? "Could not edit message."));
        }

        var message = await _context.Messages.AsNoTracking().FirstOrDefaultAsync(m => m.Id == messageId);
        if (message != null)
        {
            await _chatHubContext.Clients.Group(message.ConversationId.ToString())
                .SendAsync("MessageEdited", messageId, content);
        }

        return Ok(ApiResponse<bool>.Ok(result.Value, "Message edited successfully."));
    }

    [HttpDelete("{messageId:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteMessage(
        Guid messageId
    )
    {
        var message = await _context.Messages.AsNoTracking().FirstOrDefaultAsync(m => m.Id == messageId);
        if (message == null)
        {
            return NotFound(ApiResponse<bool>.Failure("Message not found."));
        }

        var command = new DeleteMessageCommand(messageId);
        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<bool>.Failure(result.Error ?? "Could not delete message."));
        }

        await _chatHubContext.Clients.Group(message.ConversationId.ToString())
            .SendAsync("MessageDeleted", messageId);

        return Ok(ApiResponse<bool>.Ok(result.Value, "Message deleted successfully."));
    }

    [HttpPost("conversation/{conversationId:guid}/read")]
    public async Task<ActionResult<ApiResponse<bool>>> MarkAsRead(
        Guid conversationId
    )
    {
        var command = new MarkAsReadCommand(conversationId);
        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<bool>.Failure(result.Error ?? "Could not mark conversation as read."));
        }

        return Ok(ApiResponse<bool>.Ok(result.Value, "Conversation marked as read successfully."));
    }

    [HttpGet("conversations")]
    public async Task<ActionResult<ApiResponse<PaginatedList<ConversationDto>>>> GetConversations(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10
    )
    {
        var query = new GetConversationsQuery(pageNumber, pageSize);
        var result = await Mediator.Send(query);
        return Ok(ApiResponse<PaginatedList<ConversationDto>>.Ok(result, "Conversations retrieved successfully."));
    }

    [HttpGet("conversation/{conversationId:guid}")]
    public async Task<ActionResult<ApiResponse<PaginatedList<MessageDto>>>> GetMessages(
        Guid conversationId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20
    )
    {
        var query = new GetMessagesQuery(conversationId, pageNumber, pageSize);
        var result = await Mediator.Send(query);
        return Ok(ApiResponse<PaginatedList<MessageDto>>.Ok(result, "Messages retrieved successfully."));
    }
}
