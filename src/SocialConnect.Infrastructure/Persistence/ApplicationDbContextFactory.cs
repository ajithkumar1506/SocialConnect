using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using SocialConnect.Infrastructure.Persistence.Interceptors;

namespace SocialConnect.Infrastructure.Persistence;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        // Use a dummy connection string for design-time generation.
        // It's sufficient for EF Core to build the model and generate migrations.
        optionsBuilder.UseSqlServer(
            "Server=.;Database=SocialConnectDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
        );

        // The interceptor needs an IDateTime provider. We can mock it for design time.
        var mockDateTime = new DesignTimeDateTime();
        var interceptor = new AuditableEntitySaveChangesInterceptor(mockDateTime);

        var mockCurrentUserService = new DesignTimeCurrentUserService();
        var mockHttpContextAccessor = new Microsoft.AspNetCore.Http.HttpContextAccessor();
        var auditInterceptor = new AuditLoggingInterceptor(mockCurrentUserService, mockHttpContextAccessor);

        return new ApplicationDbContext(optionsBuilder.Options, interceptor, auditInterceptor);
    }
}

internal class DesignTimeDateTime : Application.Common.Interfaces.IDateTime
{
    public DateTimeOffset Now => DateTimeOffset.UtcNow;
}

internal class DesignTimeCurrentUserService : Application.Common.Interfaces.ICurrentUserService
{
    public Guid? UserId => null;
    public string? Email => null;
    public string? IpAddress => null;
}
