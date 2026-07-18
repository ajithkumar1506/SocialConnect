using System.Linq.Expressions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Moq;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Domain.Entities.Posts;
using SocialConnect.Domain.Enums;
using SocialConnect.Domain.Repositories;
using SocialConnect.Domain.ValueObjects;
using SocialConnect.Infrastructure.BackgroundJobs;

namespace SocialConnect.Infrastructure.Tests.BackgroundJobs;

public class ScheduledPostPublishJobTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<ScheduledPostPublishJob>> _loggerMock;

    public ScheduledPostPublishJobTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<ScheduledPostPublishJob>>();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPublishScheduledPosts()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        var post1 = Post.Create(authorId, PostContent.Create("Post 1"), PostType.Text, PostStatus.Scheduled, now.AddMinutes(-5));
        var post2 = Post.Create(authorId, PostContent.Create("Post 2"), PostType.Text, PostStatus.Scheduled, now.AddMinutes(5)); // future
        var post3 = Post.Create(authorId, PostContent.Create("Post 3"), PostType.Text, PostStatus.Published, now.AddMinutes(-10)); // already published

        var postsList = new List<Post> { post1, post2, post3 };
        var dbSet = CreateMockDbSet(postsList);

        _contextMock.Setup(x => x.Posts).Returns(dbSet);

        var job = new ScheduledPostPublishJob(
            _contextMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object
        );

        // Act
        await job.ExecuteAsync(CancellationToken.None);

        // Assert
        post1.Status.Should().Be(PostStatus.Published);
        post1.PublishedAt.Should().NotBeNull();

        post2.Status.Should().Be(PostStatus.Scheduled); // untouched
        post3.Status.Should().Be(PostStatus.Published); // untouched

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    public static DbSet<T> CreateMockDbSet<T>(List<T> sourceList) where T : class
    {
        var queryable = sourceList.AsQueryable();
        var dbSetMock = new Mock<DbSet<T>>();

        dbSetMock.As<IQueryable<T>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<T>(queryable.Provider));
        dbSetMock.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
        dbSetMock.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        dbSetMock.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());

        dbSetMock.As<IAsyncEnumerable<T>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<T>(sourceList.GetEnumerator()));

        return dbSetMock.Object;
    }
}

internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    internal TestAsyncQueryProvider(IQueryProvider inner)
    {
        _inner = inner;
    }

    public IQueryable CreateQuery(Expression expression)
    {
        var elementType = expression.Type.GetInterfaces()
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            .Select(i => i.GetGenericArguments()[0])
            .FirstOrDefault() ?? typeof(TEntity);

        var testQueryType = typeof(TestAsyncEnumerable<>).MakeGenericType(elementType);
        return (IQueryable)Activator.CreateInstance(testQueryType, new object[] { expression })!;
    }
    public IQueryable<TElement> CreateQuery<TElement>(Expression expression) => new TestAsyncEnumerable<TElement>(expression);
    public object? Execute(Expression expression) => _inner.Execute(expression);
    public TResult Execute<TResult>(Expression expression) => _inner.Execute<TResult>(expression);
    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
    {
        var expectedResultType = typeof(TResult).GetGenericArguments()[0];
        var executionResult = typeof(IQueryProvider)
            .GetMethods()
            .First(m => m.Name == nameof(IQueryProvider.Execute) && m.IsGenericMethod)
            .MakeGenericMethod(expectedResultType)
            .Invoke(_inner, new[] { expression });

        return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))!
            .MakeGenericMethod(expectedResultType)
            .Invoke(null, new[] { executionResult })!;
    }
}

internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncEnumerable(Expression expression) : base(expression) { }
    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) => new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
}

internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public TestAsyncEnumerator(IEnumerator<T> inner)
    {
        _inner = inner;
    }

    public ValueTask DisposeAsync()
    {
        _inner.Dispose();
        return ValueTask.CompletedTask;
    }

    public ValueTask<bool> MoveNextAsync() => ValueTask.FromResult(_inner.MoveNext());
    public T Current => _inner.Current;
}
