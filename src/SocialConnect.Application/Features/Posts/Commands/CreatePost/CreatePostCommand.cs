using MediatR;
using SocialConnect.Application.Common.Models;
using SocialConnect.Application.Features.Posts.DTOs;
using SocialConnect.Domain.Enums;

namespace SocialConnect.Application.Features.Posts.Commands.CreatePost;

public record CreatePostCommand(
    string? Content,
    PostType PostType,
    PostStatus Status,
    List<MediaItemDto>? MediaItems
) : IRequest<Result<Guid>>;
