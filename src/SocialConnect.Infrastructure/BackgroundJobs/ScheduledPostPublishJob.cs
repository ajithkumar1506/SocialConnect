using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Domain.Enums;
using SocialConnect.Domain.Repositories;

namespace SocialConnect.Infrastructure.BackgroundJobs;

public class ScheduledPostPublishJob
{
    private readonly IApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ScheduledPostPublishJob> _logger;

    public ScheduledPostPublishJob(
        IApplicationDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<ScheduledPostPublishJob> logger
    )
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var now = DateTimeOffset.UtcNow;

            var postsToPublish = await _context.Posts
                .Where(p => p.Status == PostStatus.Scheduled && p.ScheduledAt <= now)
                .ToListAsync(cancellationToken);

            if (!postsToPublish.Any())
            {
                return;
            }

            _logger.LogInformation("Found {Count} scheduled posts to publish.", postsToPublish.Count);

            foreach (var post in postsToPublish)
            {
                post.Publish();
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully published {Count} posts.", postsToPublish.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while executing ScheduledPostPublishJob.");
            throw;
        }
    }
}
