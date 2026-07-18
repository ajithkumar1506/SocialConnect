using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SocialConnect.Application.BackgroundJobs;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Domain.Repositories;
using SocialConnect.Infrastructure.BackgroundJobs;
using SocialConnect.Infrastructure.Persistence;
using SocialConnect.Infrastructure.Persistence.Interceptors;
using SocialConnect.Infrastructure.Persistence.Repositories;
using SocialConnect.Infrastructure.Services;

namespace SocialConnect.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddScoped<AuditableEntitySaveChangesInterceptor>();
        services.AddScoped<AuditLoggingInterceptor>();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            var sqliteConnection = serviceProvider.GetService<Microsoft.Data.Sqlite.SqliteConnection>();
            if (sqliteConnection != null)
            {
                options.UseSqlite(sqliteConnection);
            }
            else
            {
                options.UseSqlServer(
                    connectionString,
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)
                );
            }
            options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        });

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>()
        );

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPostRepository, PostRepository>();
        services.AddScoped<IConversationRepository, ConversationRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<ICommentRepository, CommentRepository>();
        services.AddScoped<IReactionRepository, ReactionRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IFollowRepository, FollowRepository>();

        services.AddTransient<IDateTime, DateTimeService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IFileStorageService, S3FileStorageService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IBackgroundJobService, BackgroundJobService>();

        services.AddScoped<EmailSendingJob>();
        services.AddScoped<NotificationProcessingJob>();
        services.AddScoped<ThumbnailGenerationJob>();
        services.AddScoped<ScheduledPostPublishJob>();
        services.AddScoped<CleanupExpiredTokensJob>();
        services.AddScoped<CleanupOrphanUploadsJob>();
        services.AddHostedService<BackgroundJobsRegistrationHostedService>();

        services.AddDistributedMemoryCache();
        services.AddScoped<ICacheService, CacheService>();

        var redisConnectionString = configuration.GetSection("RedisSettings")["ConnectionString"] ?? "localhost:6379";
        services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(sp =>
            StackExchange.Redis.ConnectionMultiplexer.Connect(redisConnectionString));

        var awsOptions = configuration.GetAWSOptions();
        services.AddDefaultAWSOptions(awsOptions);
        services.AddAWSService<Amazon.S3.IAmazonS3>();

        services.AddSingleton<IEventBus, RabbitMqEventBus>();

        services.AddHangfire(config =>
            config
                .SetDataCompatibilityLevel(Hangfire.CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseInMemoryStorage()
        );

        services.AddHangfireServer(options =>
        {
            options.WorkerCount = 1;
        });

        return services;
    }
}
