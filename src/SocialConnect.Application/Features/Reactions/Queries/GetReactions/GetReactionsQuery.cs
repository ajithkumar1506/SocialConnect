using MediatR;
using SocialConnect.Application.Common.Models;
using SocialConnect.Application.Features.Reactions.DTOs;
using SocialConnect.Domain.Enums;

namespace SocialConnect.Application.Features.Reactions.Queries.GetReactions;

public record GetReactionsQuery(
    Guid TargetId,
    TargetType TargetType,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<PaginatedList<ReactionDto>>;
