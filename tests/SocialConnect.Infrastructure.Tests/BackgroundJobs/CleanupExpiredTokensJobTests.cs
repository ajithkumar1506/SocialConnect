using Microsoft.EntityFrameworkCore;
using Moq;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Domain.Entities.Users;
using SocialConnect.Domain.Repositories;
using SocialConnect.Infrastructure.BackgroundJobs;

namespace SocialConnect.Infrastructure.Tests.BackgroundJobs;

public class CleanupExpiredTokensJobTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<Microsoft.Extensions.Logging.ILogger<CleanupExpiredTokensJob>> _loggerMock;

    public CleanupExpiredTokensJobTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<CleanupExpiredTokensJob>>();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCleanUpExpiredTokens()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        var token1 = RefreshToken.Create(userId, "token1", "jwt1", now.AddMinutes(-5), "127.0.0.1", null);
        var token2 = RefreshToken.Create(userId, "token2", "jwt2", now.AddMinutes(5), "127.0.0.1", null); // active

        var tokensList = new List<RefreshToken> { token1, token2 };
        var mockDbSet = ScheduledPostPublishJobTests.CreateMockDbSet(tokensList);

        _contextMock.Setup(x => x.RefreshTokens).Returns(mockDbSet);

        var job = new CleanupExpiredTokensJob(
            _contextMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object
        );

        // Act
        await job.ExecuteAsync(CancellationToken.None);

        // Assert
        _contextMock.Verify(x => x.RefreshTokens.RemoveRange(It.Is<IEnumerable<RefreshToken>>(
            list => list.Contains(token1) && !list.Contains(token2)
        )), Times.Once);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
