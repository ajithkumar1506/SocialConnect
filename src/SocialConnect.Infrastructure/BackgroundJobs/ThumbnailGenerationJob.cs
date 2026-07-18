using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Domain.Repositories;

namespace SocialConnect.Infrastructure.BackgroundJobs;

public class ThumbnailGenerationJob
{
    private readonly IAmazonS3 _s3Client;
    private readonly IApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ThumbnailGenerationJob> _logger;
    private readonly string _bucketName;

    public ThumbnailGenerationJob(
        IAmazonS3 s3Client,
        IApplicationDbContext context,
        IUnitOfWork unitOfWork,
        IConfiguration configuration,
        ILogger<ThumbnailGenerationJob> logger
    )
    {
        _s3Client = s3Client;
        _context = context;
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _logger = logger;
        _bucketName = _configuration["AWS:BucketName"] ?? "socialconnect-local-bucket";
    }

    public async Task GenerateThumbnailAsync(Guid entityId, string entityType, string imageUrl)
    {
        try
        {
            _logger.LogInformation("Starting thumbnail generation for {EntityType} {EntityId}", entityType, entityId);

            // 1. Extract Key
            var key = ExtractKeyFromUrl(imageUrl);

            // 2. Download original image from S3
            using var getResponse = await _s3Client.GetObjectAsync(new GetObjectRequest
            {
                BucketName = _bucketName,
                Key = key
            });

            using var imageStream = getResponse.ResponseStream;
            using var memoryStream = new MemoryStream();
            await imageStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            // 3. Load and Resize image using ImageSharp
            using var image = await Image.LoadAsync(memoryStream);

            int targetSize = entityType.Equals("PostMedia", StringComparison.OrdinalIgnoreCase) ? 400 : 150;

            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new Size(targetSize, targetSize)
            }));

            // 4. Save resized image to output stream
            using var outputStream = new MemoryStream();
            await image.SaveAsJpegAsync(outputStream);
            outputStream.Position = 0;

            // 5. Upload thumbnail back to S3
            var thumbnailKey = $"thumb_{key}";
            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = outputStream,
                Key = thumbnailKey,
                BucketName = _bucketName,
                ContentType = "image/jpeg",
                CannedACL = S3CannedACL.PublicRead
            };

            using var transferUtility = new TransferUtility(_s3Client);
            await transferUtility.UploadAsync(uploadRequest);

            // 6. Construct thumbnail URL
            var serviceUrl = _configuration["AWS:ServiceURL"];
            string thumbnailUrl = !string.IsNullOrEmpty(serviceUrl)
                ? $"{serviceUrl.TrimEnd('/')}/{_bucketName}/{thumbnailKey}"
                : $"https://{_bucketName}.s3.amazonaws.com/{thumbnailKey}";

            // 7. Update Database Reference
            if (entityType.Equals("PostMedia", StringComparison.OrdinalIgnoreCase))
            {
                var postMedia = await _context.PostMedia.FirstOrDefaultAsync(pm => pm.Id == entityId);
                if (postMedia != null)
                {
                    postMedia.UpdateThumbnailUrl(thumbnailUrl);
                }
            }
            else if (entityType.Equals("MessageAttachment", StringComparison.OrdinalIgnoreCase))
            {
                var attachment = await _context.MessageAttachments.FirstOrDefaultAsync(ma => ma.Id == entityId);
                if (attachment != null)
                {
                    attachment.UpdateThumbnailUrl(thumbnailUrl);
                }
            }

            await _unitOfWork.SaveChangesAsync(CancellationToken.None);

            _logger.LogInformation("Successfully generated and updated thumbnail for {EntityType} {EntityId}", entityType, entityId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during thumbnail generation for {EntityType} {EntityId}", entityType, entityId);
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
