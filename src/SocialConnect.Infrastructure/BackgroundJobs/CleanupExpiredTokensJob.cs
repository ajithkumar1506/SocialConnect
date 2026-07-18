using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Domain.Repositories;

namespace SocialConnect.Infrastructure.BackgroundJobs;

public class CleanupExpiredTokensJob
{
    private readonly IApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CleanupExpiredTokensJob> _logger;

    public CleanupExpiredTokensJob(
        IApplicationDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<CleanupExpiredTokensJob> logger
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

            var expiredTokens = await _context.RefreshTokens
                .Where(t => t.ExpiresAt <= now)
                .ToListAsync(cancellationToken);

            if (!expiredTokens.Any())
            {
                return;
            }

            _logger.LogInformation("Found {Count} expired refresh tokens to clean up.", expiredTokens.Count);

            _context.RefreshTokens.RemoveRange(expiredTokens);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully cleaned up {Count} expired refresh tokens.", expiredTokens.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while executing CleanupExpiredTokensJob.");
            throw;
        }
    }
}
