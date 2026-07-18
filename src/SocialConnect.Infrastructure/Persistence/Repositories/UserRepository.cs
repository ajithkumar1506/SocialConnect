using Microsoft.EntityFrameworkCore;
using SocialConnect.Domain.Entities.Users;
using SocialConnect.Domain.Repositories;

namespace SocialConnect.Infrastructure.Persistence.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext dbContext)
        : base(dbContext) { }

    public async Task<User?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default
    )
    {
        return await DbSet
            .Include(u => u.Profile)
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(
                u => u.NormalizedEmail == email.ToUpperInvariant(),
                cancellationToken
            );
    }

    public async Task<User?> GetByUserNameAsync(
        string userName,
        CancellationToken cancellationToken = default
    )
    {
        return await DbSet
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.UserName == userName, cancellationToken);
    }

    public async Task<bool> IsEmailUniqueAsync(
        string email,
        CancellationToken cancellationToken = default
    )
    {
        return !await DbSet.AnyAsync(
            u => u.NormalizedEmail == email.ToUpperInvariant(),
            cancellationToken
        );
    }

    public async Task<bool> IsUserNameUniqueAsync(
        string userName,
        CancellationToken cancellationToken = default
    )
    {
        return !await DbSet.AnyAsync(u => u.UserName == userName, cancellationToken);
    }
}
