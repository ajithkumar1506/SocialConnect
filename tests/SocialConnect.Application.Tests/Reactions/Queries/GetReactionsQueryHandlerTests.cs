using AutoMapper;
using FluentAssertions;
using Moq;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Common.Mappings;
using SocialConnect.Application.Features.Reactions.Queries.GetReactions;
using SocialConnect.Application.Tests.Users.Commands;
using SocialConnect.Domain.Entities.Posts;
using SocialConnect.Domain.Entities.Users;
using SocialConnect.Domain.Enums;
using SocialConnect.Domain.ValueObjects;

namespace SocialConnect.Application.Tests.Reactions.Queries;

public class GetReactionsQueryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly IMapper _mapper;

    public GetReactionsQueryHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        _mapper = mapperConfig.CreateMapper();
    }

    [Fact]
    public async Task Handle_WithValidTarget_ShouldReturnPaginatedReactions()
    {
        // Arrange
        var targetId = Guid.NewGuid();
        var user = User.Create(Email.Create("user@example.com"), "john_doe", Password.Create("hash"));
        var profile = UserProfile.Create(user.Id, "John", "Doe");
        user.SetProfile(profile);

        var reaction1 = Reaction.Create(user.Id, targetId, TargetType.Post, ReactionType.Like);
        // Use reflection to set User navigation property
        typeof(Reaction).GetProperty("User")?.SetValue(reaction1, user);

        var reactionsList = new List<Reaction> { reaction1 };
        var dbSet = DbSetMockHelper.CreateMockDbSet(reactionsList);
        _contextMock.Setup(x => x.Reactions).Returns(dbSet);

        var handler = new GetReactionsQueryHandler(_contextMock.Object, _mapper);
        var query = new GetReactionsQuery(targetId, TargetType.Post, 1, 10);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().UserName.Should().Be("john_doe");
        result.Items.First().UserFirstName.Should().Be("John");
        result.Items.First().ReactionType.Should().Be(ReactionType.Like);
    }
}
