using AutoMapper;
using FluentAssertions;
using Moq;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Common.Mappings;
using SocialConnect.Application.Features.Posts.Queries.SearchPosts;
using SocialConnect.Application.Tests.Users.Commands;
using SocialConnect.Domain.Entities.Posts;
using SocialConnect.Domain.Entities.Social;
using SocialConnect.Domain.Enums;
using SocialConnect.Domain.ValueObjects;

namespace SocialConnect.Application.Tests.Posts.Queries;

public class SearchPostsQueryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly IMapper _mapper;

    public SearchPostsQueryHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        _mapper = mapperConfig.CreateMapper();
    }

    [Fact]
    public async Task Handle_WithValidSearchTerm_ShouldReturnMatchingPosts()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var post1 = Post.Create(authorId, PostContent.Create("Hello World!"), PostType.Text, PostStatus.Published);
        var post2 = Post.Create(authorId, PostContent.Create("Goodbye World!"), PostType.Text, PostStatus.Published);

        var postsList = new List<Post> { post1, post2 };
        var postsDbSet = DbSetMockHelper.CreateMockDbSet(postsList);
        _contextMock.Setup(x => x.Posts).Returns(postsDbSet);

        // No blocks or mutes
        _contextMock.Setup(x => x.Blocks).Returns(DbSetMockHelper.CreateMockDbSet(new List<Block>()));
        _contextMock.Setup(x => x.Mutes).Returns(DbSetMockHelper.CreateMockDbSet(new List<Mute>()));

        var handler = new SearchPostsQueryHandler(_contextMock.Object, _currentUserServiceMock.Object, _mapper);
        var query = new SearchPostsQuery("hello", PageNumber: 1, PageSize: 10);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().Content.Should().Be("Hello World!");
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithBlockedAuthor_ShouldExcludePosts()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var blockedAuthorId = Guid.NewGuid();

        var post = Post.Create(blockedAuthorId, PostContent.Create("Secret post"), PostType.Text, PostStatus.Published);
        var postsList = new List<Post> { post };
        var postsDbSet = DbSetMockHelper.CreateMockDbSet(postsList);
        _contextMock.Setup(x => x.Posts).Returns(postsDbSet);

        // Setup block relationship
        var block = Block.Create(currentUserId, blockedAuthorId);
        var blocksList = new List<Block> { block };
        var blocksDbSet = DbSetMockHelper.CreateMockDbSet(blocksList);
        _contextMock.Setup(x => x.Blocks).Returns(blocksDbSet);
        _contextMock.Setup(x => x.Mutes).Returns(DbSetMockHelper.CreateMockDbSet(new List<Mute>()));

        _currentUserServiceMock.Setup(x => x.UserId).Returns(currentUserId);

        var handler = new SearchPostsQueryHandler(_contextMock.Object, _currentUserServiceMock.Object, _mapper);
        var query = new SearchPostsQuery("secret", PageNumber: 1, PageSize: 10);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }
}
