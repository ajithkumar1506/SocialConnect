using SocialConnect.Domain.Entities.Users;

namespace SocialConnect.Domain.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);
    Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> IsUserNameUniqueAsync(
        string userName,
        CancellationToken cancellationToken = default
    );
}
