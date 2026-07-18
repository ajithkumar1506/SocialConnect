using FluentAssertions;
using Moq;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Features.Comments.Commands.UpdateComment;
using SocialConnect.Application.Tests.Users.Commands;
using SocialConnect.Domain.Entities.Posts;

namespace SocialConnect.Application.Tests.Comments.Commands;

public class UpdateCommentCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;

    public UpdateCommentCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
    }

    [Fact]
    public async Task Handle_WithValidAuthor_ShouldUpdateAndReturnSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var comment = Comment.Create(Guid.NewGuid(), userId, "Old content", null, string.Empty, 0);
        var commentsList = new List<Comment> { comment };
        var dbSet = DbSetMockHelper.CreateMockDbSet(commentsList);

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _contextMock.Setup(x => x.Comments).Returns(dbSet);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new UpdateCommentCommandHandler(_contextMock.Object, _currentUserServiceMock.Object);
        var command = new UpdateCommentCommand(comment.Id, "New content");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        comment.Content.Should().Be("New content");
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonAuthor_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var comment = Comment.Create(Guid.NewGuid(), authorId, "Old content", null, string.Empty, 0);
        var commentsList = new List<Comment> { comment };
        var dbSet = DbSetMockHelper.CreateMockDbSet(commentsList);

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _contextMock.Setup(x => x.Comments).Returns(dbSet);

        var handler = new UpdateCommentCommandHandler(_contextMock.Object, _currentUserServiceMock.Object);
        var command = new UpdateCommentCommand(comment.Id, "New content");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("You are not authorized to update this comment.");
        comment.Content.Should().Be("Old content");
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
