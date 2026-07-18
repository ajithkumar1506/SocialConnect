using FluentAssertions;
using Moq;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Features.Posts.Commands.SchedulePost;
using SocialConnect.Application.Tests.Users.Commands;
using SocialConnect.Domain.Entities.Posts;
using SocialConnect.Domain.Enums;

namespace SocialConnect.Application.Tests.Posts.Commands;

public class SchedulePostCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;

    public SchedulePostCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldScheduleAndReturnSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var post = Post.Create(userId, null, PostType.Text, PostStatus.Draft);
        var postsList = new List<Post> { post };
        var dbSet = DbSetMockHelper.CreateMockDbSet(postsList);

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _contextMock.Setup(x => x.Posts).Returns(dbSet);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new SchedulePostCommandHandler(_contextMock.Object, _currentUserServiceMock.Object);
        var scheduleTime = DateTimeOffset.UtcNow.AddHours(2);
        var command = new SchedulePostCommand(post.Id, scheduleTime);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        post.Status.Should().Be(PostStatus.Scheduled);
        post.ScheduledAt.Should().Be(scheduleTime);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithAlreadyPublishedPost_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var post = Post.Create(userId, null, PostType.Text, PostStatus.Published);
        var postsList = new List<Post> { post };
        var dbSet = DbSetMockHelper.CreateMockDbSet(postsList);

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _contextMock.Setup(x => x.Posts).Returns(dbSet);

        var handler = new SchedulePostCommandHandler(_contextMock.Object, _currentUserServiceMock.Object);
        var command = new SchedulePostCommand(post.Id, DateTimeOffset.UtcNow.AddHours(2));

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Cannot schedule");
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
