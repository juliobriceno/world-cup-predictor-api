using Goal2026API.DTOs.Storage;

namespace Goal2026API.Services;

public interface IStorageService
{
    bool IsAllowedContentType(string contentType);
    void ValidateUploadRequest(string contentType, long fileSize);

    string BuildTemporaryGroupImageKey(string firebaseUid, string contentType);
    string BuildFinalGroupImageKey(int groupId, string contentType);
    string BuildUserImageKey(int userId, string contentType);

    Task<ImageUploadTicketDto> CreateUploadTicketAsync(
        string objectKey,
        string contentType,
        long fileSize,
        CancellationToken cancellationToken);

    Task<ImageReadUrlDto> CreateReadUrlAsync(
        string objectKey,
        CancellationToken cancellationToken);

}