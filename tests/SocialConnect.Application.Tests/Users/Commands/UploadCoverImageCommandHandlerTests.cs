using FluentAssertions;
using Moq;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Features.Users.Commands.UploadCoverImage;
using SocialConnect.Domain.Entities.Users;

namespace SocialConnect.Application.Tests.Users.Commands;

public class UploadCoverImageCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IFileStorageService> _fileStorageServiceMock;

    public UploadCoverImageCommandHandlerTests()
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
        profile.UpdateCoverImage("http://old-url.com/cover.jpg");
        var profilesList = new List<UserProfile> { profile };
        var dbSet = DbSetMockHelper.CreateMockDbSet(profilesList);

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _contextMock.Setup(x => x.UserProfiles).Returns(dbSet);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var fileStream = new MemoryStream();
        var command = new UploadCoverImageCommand(fileStream, "new-cover.png", "image/png");

        _fileStorageServiceMock
            .Setup(x => x.UploadFileAsync(fileStream, "new-cover.png", "image/png", It.IsAny<CancellationToken>()))
            .ReturnsAsync("http://new-url.com/cover.png");

        var handler = new UploadCoverImageCommandHandler(
            _contextMock.Object,
            _currentUserServiceMock.Object,
            _fileStorageServiceMock.Object
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("http://new-url.com/cover.png");
        profile.CoverImageUrl.Should().Be("http://new-url.com/cover.png");

        _fileStorageServiceMock.Verify(x => x.DeleteFileAsync("http://old-url.com/cover.jpg", It.IsAny<CancellationToken>()), Times.Once);
        _fileStorageServiceMock.Verify(x => x.UploadFileAsync(fileStream, "new-cover.png", "image/png", It.IsAny<CancellationToken>()), Times.Once);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidContentType_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

        var fileStream = new MemoryStream();
        var command = new UploadCoverImageCommand(fileStream, "malicious.txt", "text/plain");

        var handler = new UploadCoverImageCommandHandler(
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
