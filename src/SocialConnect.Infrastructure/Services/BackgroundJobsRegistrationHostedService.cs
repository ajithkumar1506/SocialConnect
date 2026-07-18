using Hangfire;
using Microsoft.Extensions.Hosting;
using SocialConnect.Infrastructure.BackgroundJobs;

namespace SocialConnect.Infrastructure.Services;

public class BackgroundJobsRegistrationHostedService : IHostedService
{
    private readonly IRecurringJobManager _recurringJobManager;

    public BackgroundJobsRegistrationHostedService(IRecurringJobManager recurringJobManager)
    {
        _recurringJobManager = recurringJobManager;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _recurringJobManager.AddOrUpdate<ScheduledPostPublishJob>(
            "scheduled-post-publish",
            job => job.ExecuteAsync(CancellationToken.None),
            Cron.Minutely()
        );

        _recurringJobManager.AddOrUpdate<CleanupExpiredTokensJob>(
            "cleanup-expired-tokens",
            job => job.ExecuteAsync(CancellationToken.None),
            Cron.Daily()
        );

        _recurringJobManager.AddOrUpdate<CleanupOrphanUploadsJob>(
            "cleanup-orphan-uploads",
            job => job.ExecuteAsync(CancellationToken.None),
            Cron.Daily()
        );

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
