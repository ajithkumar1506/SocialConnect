using FluentAssertions;
using Moq;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Features.Comments.Commands.DeleteComment;
using SocialConnect.Application.Tests.Users.Commands;
using SocialConnect.Domain.Entities.Posts;

namespace SocialConnect.Application.Tests.Comments.Commands;

public class DeleteCommentCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;

    public DeleteCommentCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
    }

    [Fact]
    public async Task Handle_WithValidAuthor_ShouldDeleteAndReturnSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var comment = Comment.Create(Guid.NewGuid(), userId, "Content", null, string.Empty, 0);
        var commentsList = new List<Comment> { comment };
        var dbSet = DbSetMockHelper.CreateMockDbSet(commentsList);

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _contextMock.Setup(x => x.Comments).Returns(dbSet);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new DeleteCommentCommandHandler(_contextMock.Object, _currentUserServiceMock.Object);
        var command = new DeleteCommentCommand(comment.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _contextMock.Verify(x => x.Comments.Remove(comment), Times.Once);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonAuthor_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var comment = Comment.Create(Guid.NewGuid(), authorId, "Content", null, string.Empty, 0);
        var commentsList = new List<Comment> { comment };
        var dbSet = DbSetMockHelper.CreateMockDbSet(commentsList);

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _contextMock.Setup(x => x.Comments).Returns(dbSet);

        var handler = new DeleteCommentCommandHandler(_contextMock.Object, _currentUserServiceMock.Object);
        var command = new DeleteCommentCommand(comment.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("You are not authorized to delete this comment.");
        _contextMock.Verify(x => x.Comments.Remove(comment), Times.Never);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
