using FluentAssertions;
using Moq;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Features.Posts.Commands.PublishPost;
using SocialConnect.Application.Tests.Users.Commands;
using SocialConnect.Domain.Entities.Posts;
using SocialConnect.Domain.Enums;

namespace SocialConnect.Application.Tests.Posts.Commands;

public class PublishPostCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;

    public PublishPostCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
    }

    [Fact]
    public async Task Handle_WithValidAuthor_ShouldPublishAndReturnSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var post = Post.Create(userId, null, PostType.Text, PostStatus.Draft);
        var postsList = new List<Post> { post };
        var dbSet = DbSetMockHelper.CreateMockDbSet(postsList);

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _contextMock.Setup(x => x.Posts).Returns(dbSet);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new PublishPostCommandHandler(_contextMock.Object, _currentUserServiceMock.Object);
        var command = new PublishPostCommand(post.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        post.Status.Should().Be(PostStatus.Published);
        post.PublishedAt.Should().NotBeNull();
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonAuthor_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var post = Post.Create(authorId, null, PostType.Text, PostStatus.Draft);
        var postsList = new List<Post> { post };
        var dbSet = DbSetMockHelper.CreateMockDbSet(postsList);

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _contextMock.Setup(x => x.Posts).Returns(dbSet);

        var handler = new PublishPostCommandHandler(_contextMock.Object, _currentUserServiceMock.Object);
        var command = new PublishPostCommand(post.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("You are not authorized to publish this post.");
        post.Status.Should().Be(PostStatus.Draft);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
