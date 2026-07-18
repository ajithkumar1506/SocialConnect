using MediatR;
using SocialConnect.Application.Common.Models;
using SocialConnect.Application.Features.Messaging.DTOs;

namespace SocialConnect.Application.Features.Messaging.Queries.GetConversations;

public record GetConversationsQuery(int PageNumber = 1, int PageSize = 10) : IRequest<PaginatedList<ConversationDto>>;
