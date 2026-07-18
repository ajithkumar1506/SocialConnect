using FluentAssertions;
using Moq;
using SocialConnect.Application.Common.Interfaces;
using SocialConnect.Application.Features.Posts.Commands.UploadPostMedia;
using SocialConnect.Domain.Enums;

namespace SocialConnect.Application.Tests.Posts.Commands;

public class UploadPostMediaCommandHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IFileStorageService> _fileStorageServiceMock;

    public UploadPostMediaCommandHandlerTests()
    {
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _fileStorageServiceMock = new Mock<IFileStorageService>();
    }

    private Stream CreatePngStream(int width, int height)
    {
        var stream = new MemoryStream();
        var signature = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        stream.Write(signature, 0, 8);
        var chunkLength = new byte[] { 0, 0, 0, 13 };
        stream.Write(chunkLength, 0, 4);
        var chunkType = new byte[] { 0x49, 0x48, 0x44, 0x52 };
        stream.Write(chunkType, 0, 4);

        var w = BitConverter.GetBytes(width);
        if (BitConverter.IsLittleEndian) Array.Reverse(w);
        stream.Write(w, 0, 4);

        var h = BitConverter.GetBytes(height);
        if (BitConverter.IsLittleEndian) Array.Reverse(h);
        stream.Write(h, 0, 4);

        // Fill up stream so Length returns something valid
        stream.Write(new byte[100], 0, 100);
        stream.Position = 0;
        return stream;
    }

    [Fact]
    public async Task Handle_WithValidImageDimensions_ShouldUploadAndReturnDto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

        var stream = CreatePngStream(1024, 768);
        var command = new UploadPostMediaCommand(stream, "image.png", "image/png");

        _fileStorageServiceMock
            .Setup(x => x.UploadFileAsync(stream, "image.png", "image/png", It.IsAny<CancellationToken>()))
            .ReturnsAsync("http://s3.local/image.png");

        var handler = new UploadPostMediaCommandHandler(_currentUserServiceMock.Object, _fileStorageServiceMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Url.Should().Be("http://s3.local/image.png");
        result.Value.Type.Should().Be(MediaType.Image);
    }

    [Fact]
    public async Task Handle_WithOversizedImageDimensions_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

        var stream = CreatePngStream(5000, 3000); // Exceeds 4096px limit
        var command = new UploadPostMediaCommand(stream, "big-image.png", "image/png");

        var handler = new UploadPostMediaCommandHandler(_currentUserServiceMock.Object, _fileStorageServiceMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Image dimensions exceed the maximum allowed size");
        _fileStorageServiceMock.Verify(x => x.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
