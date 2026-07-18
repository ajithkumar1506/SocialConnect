using Microsoft.EntityFrameworkCore;
using SocialConnect.Infrastructure.Persistence;

namespace SocialConnect.API.Configuration;

public class DatabaseMigrationConfigurator
{
    private readonly WebApplication _app;

    public DatabaseMigrationConfigurator(WebApplication app)
    {
        _app = app;
    }

    public async Task ApplyMigrationsAsync()
    {
        if (_app.Environment.IsDevelopment())
        {
            using var scope = _app.Services.CreateScope();
            var services = scope.ServiceProvider;
            try
            {
                var context = services.GetRequiredService<ApplicationDbContext>();
                await context.Database.MigrateAsync();
                _app.Logger.LogInformation("Database migration applied successfully.");
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<DatabaseMigrationConfigurator>>();
                logger.LogError(ex, "An error occurred while migrating the database.");
            }
        }
    }
}
