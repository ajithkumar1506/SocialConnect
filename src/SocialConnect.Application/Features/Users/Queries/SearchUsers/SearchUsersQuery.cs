using MediatR;
using SocialConnect.Application.Common.Models;
using SocialConnect.Application.Features.Users.DTOs;

namespace SocialConnect.Application.Features.Users.Queries.SearchUsers;

public record SearchUsersQuery(
    string SearchTerm,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<PaginatedList<UserProfileDto>>;
