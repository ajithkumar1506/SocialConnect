using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialConnect.Domain.Entities.Posts;
using SocialConnect.Domain.ValueObjects;

namespace SocialConnect.Infrastructure.Persistence.Configurations;

public class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.HasKey(p => p.Id);

        builder
            .Property(p => p.Content)
            .HasConversion(
                content => content != null ? content.Value : null,
                value => value != null ? PostContent.Create(value) : null
            )
            .HasMaxLength(PostContent.MaxLength);

        builder
            .HasOne(p => p.Author)
            .WithMany()
            .HasForeignKey(p => p.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasMany(p => p.Media)
            .WithOne(m => m.Post)
            .HasForeignKey(m => m.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(p => p.Comments)
            .WithOne(c => c.Post)
            .HasForeignKey(c => c.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(p => p.Reactions)
            .WithOne()
            .HasForeignKey(r => r.TargetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(p => !p.IsDeleted);

        builder.HasIndex(p => new
        {
            p.Status,
            p.IsDeleted,
            p.PublishedAt,
        });
    }
}
