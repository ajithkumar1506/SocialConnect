using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SocialConnect.Application.Common.Interfaces;

namespace SocialConnect.Infrastructure.Services;

public class S3FileStorageService : IFileStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly IConfiguration _configuration;
    private readonly ILogger<S3FileStorageService> _logger;
    private readonly string _bucketName;

    public S3FileStorageService(
        IAmazonS3 s3Client,
        IConfiguration configuration,
        ILogger<S3FileStorageService> logger
    )
    {
        _s3Client = s3Client;
        _configuration = configuration;
        _logger = logger;
        _bucketName = _configuration["AWS:BucketName"] ?? "socialconnect-local-bucket";
    }

    public async Task<string> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            // Ensure bucket exists (especially useful for local S3Ninja setup)
            await EnsureBucketExistsAsync(cancellationToken);

            var key = $"{Guid.NewGuid()}_{fileName}";

            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = fileStream,
                Key = key,
                BucketName = _bucketName,
                ContentType = contentType,
                CannedACL = S3CannedACL.PublicRead, // Making it public read for easy URL access
            };

            using var fileTransferUtility = new TransferUtility(_s3Client);
            await fileTransferUtility.UploadAsync(uploadRequest, cancellationToken);

            // Construct the public URL depending on whether it's local S3Ninja or real AWS
            var serviceUrl = _configuration["AWS:ServiceURL"];
            if (!string.IsNullOrEmpty(serviceUrl))
            {
                // Local S3Ninja or MinIO URL format
                return $"{serviceUrl.TrimEnd('/')}/{_bucketName}/{key}";
            }

            // Real AWS S3 URL format
            return $"https://{_bucketName}.s3.amazonaws.com/{key}";
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "AWS S3 Error encountered on server when writing an object");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unknown encountered on server when writing an object");
            throw;
        }
    }

    public async Task DeleteFileAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            // Extract key from URL
            var uri = new Uri(fileUrl);
            string key = uri.AbsolutePath.TrimStart('/');

            // Remove bucket name from path if using path-style URLs (like S3Ninja)
            if (key.StartsWith($"{_bucketName}/"))
            {
                key = key.Substring(_bucketName.Length + 1);
            }

            var deleteObjectRequest = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = key,
            };

            await _s3Client.DeleteObjectAsync(deleteObjectRequest, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file {FileUrl} from S3", fileUrl);
            throw;
        }
    }

    private async Task EnsureBucketExistsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var response = await _s3Client.ListBucketsAsync(cancellationToken);
            if (!response.Buckets.Any(b => b.BucketName == _bucketName))
            {
                await _s3Client.PutBucketAsync(
                    new PutBucketRequest { BucketName = _bucketName, UseClientRegion = true },
                    cancellationToken
                );

                // Allow public read policy for the bucket
                var policy = $$"""
                    {
                      "Version": "2012-10-17",
                      "Statement": [
                        {
                          "Sid": "PublicReadGetObject",
                          "Effect": "Allow",
                          "Principal": "*",
                          "Action": "s3:GetObject",
                          "Resource": "arn:aws:s3:::{{_bucketName}}/*"
                        }
                      ]
                    }
                    """;

                await _s3Client.PutBucketPolicyAsync(
                    new PutBucketPolicyRequest { BucketName = _bucketName, Policy = policy },
                    cancellationToken
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Could not ensure bucket exists. It might already exist or permissions are lacking."
            );
        }
    }

    public async Task<string> GetPreSignedUploadUrlAsync(
        string fileName,
        string contentType,
        TimeSpan expiration,
        CancellationToken cancellationToken = default
    )
    {
        var key = $"{Guid.NewGuid()}_{fileName}";

        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = key,
            Verb = HttpVerb.PUT,
            Expires = DateTime.UtcNow.Add(expiration),
            ContentType = contentType,
        };

        return await _s3Client.GetPreSignedURLAsync(request);
    }
}
