using SocialConnect.Domain.Enums;

namespace SocialConnect.Application.Features.Posts.DTOs;

public record MediaItemDto(string Url, MediaType Type, long FileSize);
