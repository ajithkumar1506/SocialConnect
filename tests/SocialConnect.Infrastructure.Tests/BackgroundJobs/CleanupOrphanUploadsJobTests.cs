using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Domain.Entities.Messaging;
using SocialConnect.Domain.Entities.Posts;
using SocialConnect.Domain.Entities.Users;
using SocialConnect.Infrastructure.BackgroundJobs;

namespace SocialConnect.Infrastructure.Tests.BackgroundJobs;

public class CleanupOrphanUploadsJobTests
{
    private readonly Mock<IAmazonS3> _s3ClientMock;
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<IConfiguration> _configMock;
    private readonly Mock<ILogger<CleanupOrphanUploadsJob>> _loggerMock;

    public CleanupOrphanUploadsJobTests()
    {
        _s3ClientMock = new Mock<IAmazonS3>();
        _contextMock = new Mock<IApplicationDbContext>();
        _configMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<CleanupOrphanUploadsJob>>();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldDeleteOnlyOldOrphanUploads()
    {
        // Arrange
        _configMock.Setup(c => c["AWS:BucketName"]).Returns("socialconnect-local-bucket");
        _configMock.Setup(c => c["AWS:ServiceURL"]).Returns("http://localhost:9000");

        // DB Data
        var profile = UserProfile.Create(Guid.NewGuid(), "First", "Last");
        typeof(UserProfile).GetProperty("ProfileImageUrl")?.SetValue(profile, "http://localhost:9000/socialconnect-local-bucket/referenced.jpg");

        var profilesDbSet = ScheduledPostPublishJobTests.CreateMockDbSet(new List<UserProfile> { profile });
        var postMediaDbSet = ScheduledPostPublishJobTests.CreateMockDbSet(new List<PostMedia>());
        var messageAttachmentsDbSet = ScheduledPostPublishJobTests.CreateMockDbSet(new List<MessageAttachment>());

        _contextMock.Setup(x => x.UserProfiles).Returns(profilesDbSet);
        _contextMock.Setup(x => x.PostMedia).Returns(postMediaDbSet);
        _contextMock.Setup(x => x.MessageAttachments).Returns(messageAttachmentsDbSet);

        // S3 Objects
        var objects = new List<S3Object>
        {
            new S3Object { Key = "orphan_old.jpg", LastModified = DateTime.UtcNow.AddDays(-2) },
            new S3Object { Key = "orphan_new.jpg", LastModified = DateTime.UtcNow.AddHours(-2) }, // too new
            new S3Object { Key = "referenced.jpg", LastModified = DateTime.UtcNow.AddDays(-2) } // referenced in DB
        };

        _s3ClientMock.Setup(x => x.ListObjectsV2Async(It.IsAny<ListObjectsV2Request>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ListObjectsV2Response
            {
                S3Objects = objects,
                IsTruncated = false
            });

        var job = new CleanupOrphanUploadsJob(
            _s3ClientMock.Object,
            _contextMock.Object,
            _configMock.Object,
            _loggerMock.Object
        );

        // Act
        await job.ExecuteAsync(CancellationToken.None);

        // Assert
        // Delete should only be called for the old orphan object
        _s3ClientMock.Verify(x => x.DeleteObjectAsync(It.Is<DeleteObjectRequest>(
            req => req.Key == "orphan_old.jpg"
        ), It.IsAny<CancellationToken>()), Times.Once);

        _s3ClientMock.Verify(x => x.DeleteObjectAsync(It.Is<DeleteObjectRequest>(
            req => req.Key == "orphan_new.jpg"
        ), It.IsAny<CancellationToken>()), Times.Never);

        _s3ClientMock.Verify(x => x.DeleteObjectAsync(It.Is<DeleteObjectRequest>(
            req => req.Key == "referenced.jpg"
        ), It.IsAny<CancellationToken>()), Times.Never);
    }
}
