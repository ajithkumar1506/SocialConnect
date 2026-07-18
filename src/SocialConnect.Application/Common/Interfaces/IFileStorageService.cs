namespace SocialConnect.Application.Common.Interfaces;

public interface IFileStorageService
{
    Task<string> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default
    );
    Task DeleteFileAsync(string fileUrl, CancellationToken cancellationToken = default);
    Task<string> GetPreSignedUploadUrlAsync(
        string fileName,
        string contentType,
        TimeSpan expiresIn,
        CancellationToken cancellationToken = default
    );
}
