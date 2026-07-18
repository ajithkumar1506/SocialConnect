using SocialConnect.Domain.Common;

namespace SocialConnect.Domain.Entities.Users;

public class Role : Entity
{
    public string Name { get; private set; } = string.Empty;
    public string NormalizedName { get; private set; } = string.Empty;

    private Role() { } // EF Core

    public static Role Create(string name)
    {
        return new Role
        {
            Id = Guid.NewGuid(),
            Name = name,
            NormalizedName = name.ToUpperInvariant(),
        };
    }
}
