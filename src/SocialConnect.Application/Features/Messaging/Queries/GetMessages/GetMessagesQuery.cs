using MediatR;
using SocialConnect.Application.Common.Models;
using SocialConnect.Application.Features.Messaging.DTOs;

namespace SocialConnect.Application.Features.Messaging.Queries.GetMessages;

public record GetMessagesQuery(Guid ConversationId, int PageNumber = 1, int PageSize = 20) : IRequest<PaginatedList<MessageDto>>;
