using FluentAssertions;
using Moq;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Features.Users.Commands.UploadProfileImage;
using SocialConnect.Domain.Entities.Users;

namespace SocialConnect.Application.Tests.Users.Commands;

public class UploadProfileImageCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IFileStorageService> _fileStorageServiceMock;

    public UploadProfileImageCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _fileStorageServiceMock = new Mock<IFileStorageService>();
    }

    [Fact]
    public async Task Handle_WithValidImage_ShouldUploadAndDeleteOldImage()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var profile = UserProfile.Create(userId, "John", "Doe");
        profile.UpdateProfileImage("http://old-url.com/image.jpg");
        var profilesList = new List<UserProfile> { profile };
        var dbSet = DbSetMockHelper.CreateMockDbSet(profilesList);

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _contextMock.Setup(x => x.UserProfiles).Returns(dbSet);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var fileStream = new MemoryStream();
        var command = new UploadProfileImageCommand(fileStream, "new-avatar.png", "image/png");

        _fileStorageServiceMock
            .Setup(x => x.UploadFileAsync(fileStream, "new-avatar.png", "image/png", It.IsAny<CancellationToken>()))
            .ReturnsAsync("http://new-url.com/avatar.png");

        var handler = new UploadProfileImageCommandHandler(
            _contextMock.Object,
            _currentUserServiceMock.Object,
            _fileStorageServiceMock.Object
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("http://new-url.com/avatar.png");
        profile.ProfileImageUrl.Should().Be("http://new-url.com/avatar.png");

        _fileStorageServiceMock.Verify(x => x.DeleteFileAsync("http://old-url.com/image.jpg", It.IsAny<CancellationToken>()), Times.Once);
        _fileStorageServiceMock.Verify(x => x.UploadFileAsync(fileStream, "new-avatar.png", "image/png", It.IsAny<CancellationToken>()), Times.Once);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidContentType_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

        var fileStream = new MemoryStream();
        var command = new UploadProfileImageCommand(fileStream, "malicious.txt", "text/plain");

        var handler = new UploadProfileImageCommandHandler(
            _contextMock.Object,
            _currentUserServiceMock.Object,
            _fileStorageServiceMock.Object
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Invalid file type");
        _fileStorageServiceMock.Verify(x => x.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
