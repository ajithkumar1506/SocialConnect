using AutoMapper;
using SocialConnect.Application.Common.Mappings;
using SocialConnect.Domain.Entities.Posts;
using SocialConnect.Domain.Enums;

namespace SocialConnect.Application.Features.Posts.DTOs;

public class PostDto : IMapFrom<Post>
{
    public Guid Id { get; set; }
    public Guid AuthorId { get; set; }
    public string? Content { get; set; }
    public PostType PostType { get; set; }
    public PostStatus Status { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }
    public int ReactionCount { get; set; }
    public int CommentCount { get; set; }

    public void Mapping(Profile profile)
    {
        profile
            .CreateMap<Post, PostDto>()
            .ForMember(
                d => d.Content,
                opt => opt.MapFrom(s => s.Content != null ? s.Content.Value : null)
            );
    }
}
