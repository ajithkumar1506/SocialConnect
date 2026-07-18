using MediatR;
using SocialConnect.Application.Features.Posts.DTOs;

namespace SocialConnect.Application.Features.Posts.Queries.GetPostById;

public record GetPostByIdQuery(Guid Id) : IRequest<PostDto?>;
