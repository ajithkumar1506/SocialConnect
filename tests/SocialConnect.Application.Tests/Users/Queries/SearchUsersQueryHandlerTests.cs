using AutoMapper;
using FluentAssertions;
using Moq;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Common.Mappings;
using SocialConnect.Application.Features.Users.Queries.SearchUsers;
using SocialConnect.Application.Tests.Users.Commands;
using SocialConnect.Domain.Entities.Users;
using SocialConnect.Domain.ValueObjects;

namespace SocialConnect.Application.Tests.Users.Queries;

public class SearchUsersQueryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly IMapper _mapper;

    public SearchUsersQueryHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        _mapper = mapperConfig.CreateMapper();
    }

    [Fact]
    public async Task Handle_WithSearchTerm_ShouldReturnMatchingProfiles()
    {
        // Arrange
        var user1 = User.Create(Email.Create("alice@example.com"), "alice_w", Password.Create("hash"));
        var profile1 = UserProfile.Create(user1.Id, "Alice", "Wonderland");
        profile1.UpdateDetails("Alice", "Wonderland", "Wanderer", null, null, null);
        user1.SetProfile(profile1);

        var user2 = User.Create(Email.Create("bob@example.com"), "bob_builder", Password.Create("hash"));
        var profile2 = UserProfile.Create(user2.Id, "Bob", "Builder");
        profile2.UpdateDetails("Bob", "Builder", "Constructor", null, null, null);
        user2.SetProfile(profile2);

        var profilesList = new List<UserProfile> { profile1, profile2 };
        var dbSet = DbSetMockHelper.CreateMockDbSet(profilesList);

        _contextMock.Setup(x => x.UserProfiles).Returns(dbSet);

        var handler = new SearchUsersQueryHandler(_contextMock.Object, _mapper);
        var query = new SearchUsersQuery("alice");

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().FirstName.Should().Be("Alice");
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithEmptySearchTerm_ShouldReturnAllProfilesPaginated()
    {
        // Arrange
        var user1 = User.Create(Email.Create("alice@example.com"), "alice_w", Password.Create("hash"));
        var profile1 = UserProfile.Create(user1.Id, "Alice", "Wonder");
        user1.SetProfile(profile1);

        var user2 = User.Create(Email.Create("bob@example.com"), "bob_b", Password.Create("hash"));
        var profile2 = UserProfile.Create(user2.Id, "Bob", "Builder");
        user2.SetProfile(profile2);

        var profilesList = new List<UserProfile> { profile1, profile2 };
        var dbSet = DbSetMockHelper.CreateMockDbSet(profilesList);

        _contextMock.Setup(x => x.UserProfiles).Returns(dbSet);

        var handler = new SearchUsersQueryHandler(_contextMock.Object, _mapper);
        var query = new SearchUsersQuery(string.Empty, PageNumber: 1, PageSize: 10);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }
}
