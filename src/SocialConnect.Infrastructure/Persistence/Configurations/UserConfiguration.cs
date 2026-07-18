using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialConnect.Domain.Entities.Users;
using SocialConnect.Domain.ValueObjects;

namespace SocialConnect.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.UserName).HasMaxLength(256).IsRequired();

        builder.HasIndex(t => t.UserName).IsUnique();

        builder.Property(t => t.NormalizedEmail).HasMaxLength(256).IsRequired();

        builder.HasIndex(t => t.NormalizedEmail).IsUnique();

        // Value Object Conversions
        builder
            .Property(t => t.Email)
            .HasConversion(email => email.Value, value => Email.Create(value))
            .HasMaxLength(256)
            .IsRequired();

        builder
            .Property(t => t.PasswordHash)
            .HasConversion(password => password.Hash, value => Password.Create(value))
            .IsRequired();

        builder
            .HasOne(u => u.Profile)
            .WithOne(p => p.User)
            .HasForeignKey<UserProfile>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(u => u.RefreshTokens)
            .WithOne(rt => rt.User)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
