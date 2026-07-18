using AutoMapper;
using FluentAssertions;
using Moq;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Common.Mappings;
using SocialConnect.Application.Features.Users.Commands.UpdateProfile;
using SocialConnect.Domain.Entities.Users;

namespace SocialConnect.Application.Tests.Users.Commands;

public class UpdateProfileCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly IMapper _mapper;

    public UpdateProfileCommandHandlerTests()
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
    public async Task Handle_WithValidRequest_ShouldUpdateProfileAndReturnSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var profile = UserProfile.Create(userId, "John", "Doe");
        var profilesList = new List<UserProfile> { profile };
        var dbSet = DbSetMockHelper.CreateMockDbSet(profilesList);

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _contextMock.Setup(x => x.UserProfiles).Returns(dbSet);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new UpdateProfileCommandHandler(_contextMock.Object, _currentUserServiceMock.Object, _mapper);
        var command = new UpdateProfileCommand("Johnny", "Smith", "Software Engineer", "Bio description", "New York", new DateOnly(1990, 1, 1));

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.FirstName.Should().Be("Johnny");
        result.Value.LastName.Should().Be("Smith");
        result.Value.Headline.Should().Be("Software Engineer");
        result.Value.Bio.Should().Be("Bio description");
        result.Value.Location.Should().Be("New York");
        result.Value.DateOfBirth.Should().Be(new DateOnly(1990, 1, 1));

        profile.FirstName.Should().Be("Johnny");
        profile.LastName.Should().Be("Smith");

        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNotAuthenticated_ShouldReturnFailure()
    {
        // Arrange
        _currentUserServiceMock.Setup(x => x.UserId).Returns((Guid?)null);

        var handler = new UpdateProfileCommandHandler(_contextMock.Object, _currentUserServiceMock.Object, _mapper);
        var command = new UpdateProfileCommand("Johnny", "Smith", "Headline", "Bio", "Location", null);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Unauthorized access.");
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenProfileNotFound_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var profilesList = new List<UserProfile>(); // Empty list
        var dbSet = DbSetMockHelper.CreateMockDbSet(profilesList);

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _contextMock.Setup(x => x.UserProfiles).Returns(dbSet);

        var handler = new UpdateProfileCommandHandler(_contextMock.Object, _currentUserServiceMock.Object, _mapper);
        var command = new UpdateProfileCommand("Johnny", "Smith", "Headline", "Bio", "Location", null);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("User profile not found.");
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
