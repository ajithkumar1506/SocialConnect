using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialConnect.Domain.Entities.Posts;
using SocialConnect.Domain.ValueObjects;

namespace SocialConnect.Infrastructure.Persistence.Configurations;

public class PostMediaConfiguration : IEntityTypeConfiguration<PostMedia>
{
    public void Configure(EntityTypeBuilder<PostMedia> builder)
    {
        builder.HasKey(m => m.Id);

        builder
            .Property(m => m.MediaUrl)
            .HasConversion(url => url.Value, value => MediaUrl.Create(value))
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(m => m.ThumbnailUrl).HasMaxLength(2000);
    }
}
