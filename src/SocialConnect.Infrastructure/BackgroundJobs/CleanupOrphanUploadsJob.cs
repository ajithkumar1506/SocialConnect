using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SocialConnect.Application.Common.Interfaces;

namespace SocialConnect.Infrastructure.BackgroundJobs;

public class CleanupOrphanUploadsJob
{
    private readonly IAmazonS3 _s3Client;
    private readonly IApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CleanupOrphanUploadsJob> _logger;
    private readonly string _bucketName;

    public CleanupOrphanUploadsJob(
        IAmazonS3 s3Client,
        IApplicationDbContext context,
        IConfiguration configuration,
        ILogger<CleanupOrphanUploadsJob> logger
    )
    {
        _s3Client = s3Client;
        _context = context;
        _configuration = configuration;
        _logger = logger;
        _bucketName = _configuration["AWS:BucketName"] ?? "socialconnect-local-bucket";
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting CleanupOrphanUploadsJob execution.");

            // 1. Gather all referenced URLs from DB
            var profileImages = await _context.UserProfiles
                .Select(p => p.ProfileImageUrl)
                .Where(u => u != null)
                .ToListAsync(cancellationToken);

            var coverImages = await _context.UserProfiles
                .Select(p => p.CoverImageUrl)
                .Where(u => u != null)
                .ToListAsync(cancellationToken);

            var postMedias = await _context.PostMedia.ToListAsync(cancellationToken);
            var postMediaUrls = postMedias.Select(m => m.MediaUrl.Value).ToList();
            var postMediaThumbnails = postMedias.Select(m => m.ThumbnailUrl).Where(t => t != null).ToList();

            var messageAttachments = await _context.MessageAttachments.ToListAsync(cancellationToken);
            var attachmentUrls = messageAttachments.Select(a => a.FileUrl).ToList();
            var attachmentThumbnails = messageAttachments.Select(a => a.ThumbnailUrl).Where(t => t != null).ToList();

            // 2. Combine into referenced URLs set
            var referencedUrls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var url in profileImages) referencedUrls.Add(url!);
            foreach (var url in coverImages) referencedUrls.Add(url!);
            foreach (var url in postMediaUrls) referencedUrls.Add(url);
            foreach (var url in postMediaThumbnails) referencedUrls.Add(url!);
            foreach (var url in attachmentUrls) referencedUrls.Add(url);
            foreach (var url in attachmentThumbnails) referencedUrls.Add(url!);

            // 3. Extract keys from URLs
            var referencedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var url in referencedUrls)
            {
                try
                {
                    var key = ExtractKeyFromUrl(url);
                    referencedKeys.Add(key);
                }
                catch
                {
                    // Ignore malformed URLs
                }
            }

            // 4. List all objects in S3 and delete orphans
            var listRequest = new ListObjectsV2Request { BucketName = _bucketName };
            ListObjectsV2Response listResponse;
            int deletedCount = 0;

            do
            {
                listResponse = await _s3Client.ListObjectsV2Async(listRequest, cancellationToken);
                var cutoffTime = DateTime.UtcNow.AddDays(-1);

                foreach (var obj in listResponse.S3Objects)
                {
                    // Ignore files uploaded in the last 24 hours to prevent race conditions during upload
                    if (obj.LastModified >= cutoffTime)
                    {
                        continue;
                    }

                    if (!referencedKeys.Contains(obj.Key))
                    {
                        _logger.LogInformation("Deleting orphan S3 object: {Key}", obj.Key);
                        await _s3Client.DeleteObjectAsync(new DeleteObjectRequest
                        {
                            BucketName = _bucketName,
                            Key = obj.Key
                        }, cancellationToken);
                        deletedCount++;
                    }
                }

                listRequest.ContinuationToken = listResponse.NextContinuationToken;
            } while (listResponse.IsTruncated);

            _logger.LogInformation("CleanupOrphanUploadsJob execution completed. Deleted {Count} orphan objects.", deletedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while executing CleanupOrphanUploadsJob.");
            throw;
        }
    }

    private string ExtractKeyFromUrl(string fileUrl)
    {
        var uri = new Uri(fileUrl);
        string key = uri.AbsolutePath.TrimStart('/');

        if (key.StartsWith($"{_bucketName}/", StringComparison.OrdinalIgnoreCase))
        {
            key = key.Substring(_bucketName.Length + 1);
        }

        return key;
    }
}
