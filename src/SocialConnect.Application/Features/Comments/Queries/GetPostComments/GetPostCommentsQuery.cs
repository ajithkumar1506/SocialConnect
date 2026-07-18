using MediatR;
using SocialConnect.Application.Features.Comments.DTOs;

namespace SocialConnect.Application.Features.Comments.Queries.GetPostComments;

public record GetPostCommentsQuery(Guid PostId, int Page = 1, int PageSize = 10)
    : IRequest<List<CommentDto>>;
