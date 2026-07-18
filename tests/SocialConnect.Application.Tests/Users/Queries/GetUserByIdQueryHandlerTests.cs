using AutoMapper;
using FluentAssertions;
using Moq;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Common.Mappings;
using SocialConnect.Application.Features.Users.Queries.GetUserById;
using SocialConnect.Application.Tests.Users.Commands;
using SocialConnect.Domain.Entities.Users;
using SocialConnect.Domain.ValueObjects;

namespace SocialConnect.Application.Tests.Users.Queries;

public class GetUserByIdQueryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly IMapper _mapper;

    public GetUserByIdQueryHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        _mapper = mapperConfig.CreateMapper();
    }

    [Fact]
    public async Task Handle_WithExistingUserId_ShouldReturnUserDto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = User.Create(Email.Create("user@example.com"), "john_doe", Password.Create("hash"));
        // Use reflection or constructor to match ID
        var idProperty = typeof(User).GetProperty("Id");
        idProperty?.SetValue(user, userId);

        var profile = UserProfile.Create(userId, "John", "Doe");
        user.SetProfile(profile);

        var usersList = new List<User> { user };
        var dbSet = DbSetMockHelper.CreateMockDbSet(usersList);

        _contextMock.Setup(x => x.Users).Returns(dbSet);

        var handler = new GetUserByIdQueryHandler(_contextMock.Object, _mapper);
        var query = new GetUserByIdQuery(userId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(userId);
        result.UserName.Should().Be("john_doe");
        result.Email.Should().Be("user@example.com");
        result.Profile.Should().NotBeNull();
        result.Profile!.FirstName.Should().Be("John");
    }

    [Fact]
    public async Task Handle_WithNonExistentUserId_ShouldReturnNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var usersList = new List<User>(); // Empty
        var dbSet = DbSetMockHelper.CreateMockDbSet(usersList);

        _contextMock.Setup(x => x.Users).Returns(dbSet);

        var handler = new GetUserByIdQueryHandler(_contextMock.Object, _mapper);
        var query = new GetUserByIdQuery(userId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}
