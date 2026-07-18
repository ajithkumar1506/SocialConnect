using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using SocialConnect.Infrastructure.Persistence;

namespace SocialConnect.API.Tests;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private SqliteConnection? _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        builder.ConfigureTestServices(services =>
        {
            // Add SqliteConnection to DI so DependencyInjection.cs AddDbContext uses UseSqlite
            services.AddSingleton(_connection);

            // Register decorator options to replace IMigrator in internal EF service provider
            services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
            {
                options.UseSqlite(_connection)
                       .ReplaceService<IMigrator, SqliteTestMigrator>();
            });
        });
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public new Task DisposeAsync()
    {
        if (_connection != null)
        {
            _connection.Dispose();
        }
        return Task.CompletedTask;
    }
}

public class SqliteTestMigrator : IMigrator
{
    private readonly DbContext _context;

    public SqliteTestMigrator(ICurrentDbContext currentContext)
    {
        _context = currentContext.Context;
    }

    public void Migrate(string? targetMigration = null)
    {
        _context.Database.EnsureCreated();
    }

    public bool HasPendingModelChanges()
    {
        return false;
    }

    public Task MigrateAsync(string? targetMigration = null, CancellationToken cancellationToken = default)
    {
        _context.Database.EnsureCreated();
        return Task.CompletedTask;
    }

    public string GenerateScript(
        string? fromMigration = null,
        string? toMigration = null,
        MigrationsSqlGenerationOptions options = MigrationsSqlGenerationOptions.Default)
    {
        return string.Empty;
    }
}
