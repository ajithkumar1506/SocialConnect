using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialConnect.Domain.Entities.Social;

namespace SocialConnect.Infrastructure.Persistence.Configurations;

public class MuteConfiguration : IEntityTypeConfiguration<Mute>
{
    public void Configure(EntityTypeBuilder<Mute> builder)
    {
        builder.HasKey(m => new { m.MuterId, m.MutedId });

        builder
            .HasOne(m => m.Muter)
            .WithMany()
            .HasForeignKey(m => m.MuterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(m => m.Muted)
            .WithMany()
            .HasForeignKey(m => m.MutedId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
