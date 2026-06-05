using Amazon.S3;
using Amazon.S3.Model;
using Goal2026API.Api.Options;
using Goal2026API.DTOs.Storage;
using Goal2026API.Services;
using Microsoft.Extensions.Options;
using System.Text;

namespace Goal2026API.Api.Services;

public sealed class S3StorageService : IStorageService
{
    private readonly IAmazonS3 _s3;
    private readonly S3StorageOptions _options;

    public S3StorageService(
        IAmazonS3 s3,
        IOptions<S3StorageOptions> options)
    {
        _s3 = s3;
        _options = options.Value;
    }

    public bool IsAllowedContentType(string contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
        {
            return false;
        }

        return _options.AllowedContentTypes.Any(x =>
            string.Equals(x, contentType, StringComparison.OrdinalIgnoreCase));
    }

    public void ValidateUploadRequest(string contentType, long fileSize)
    {
        if (!IsAllowedContentType(contentType))
        {
            throw new InvalidOperationException("The file content type is not allowed.");
        }

        if (fileSize <= 0)
        {
            throw new InvalidOperationException("The file size must be greater than zero.");
        }

        if (fileSize > _options.MaxFileSizeBytes)
        {
            throw new InvalidOperationException(
                $"The file size exceeds the maximum allowed size of {_options.MaxFileSizeBytes} bytes.");
        }
    }

    public string BuildTemporaryGroupImageKey(string firebaseUid, string contentType)
    {
        var safeUid = SanitizePathSegment(firebaseUid);
        var extension = GetExtensionFromContentType(contentType);

        return $"temp/groups/{safeUid}/{Guid.NewGuid():N}{extension}";
    }

    public string BuildFinalGroupImageKey(int groupId, string contentType)
    {
        var extension = GetExtensionFromContentType(contentType);
        return $"groups/{groupId}/photo{extension}";
    }

    public string BuildUserImageKey(int userId, string contentType)
    {
        var extension = GetExtensionFromContentType(contentType);
        return $"users/{userId}/photo{extension}";
    }

    public async Task<ImageUploadTicketDto> CreateUploadTicketAsync(
        string objectKey,
        string contentType,
        long fileSize,
        CancellationToken cancellationToken)
    {
        ValidateUploadRequest(contentType, fileSize);

        var expiresAtUtc = DateTime.UtcNow.AddMinutes(_options.UploadExpirationMinutes);

        var request = new CreatePresignedPostRequest
        {
            BucketName = _options.BucketName,
            Key = objectKey,
            Expires = expiresAtUtc
        };

        request.Fields["key"] = objectKey;
        request.Fields["Content-Type"] = contentType;
        request.Fields["success_action_status"] = "201";

        request.Conditions.Add(S3PostCondition.ExactMatch("key", objectKey));
        //request.Conditions.Add(S3PostCondition.ExactMatch("Content-Type", contentType));
        request.Conditions.Add(S3PostCondition.ExactMatch("success_action_status", "201"));
        request.Conditions.Add(S3PostCondition.ContentLengthRange(1, _options.MaxFileSizeBytes));

        var response = await _s3.CreatePresignedPostAsync(request);

        return new ImageUploadTicketDto
        {
            Url = response.Url,
            Key = objectKey,
            ExpiresAtUtc = expiresAtUtc,
            Fields = response.Fields
        };
    }

    public async Task<ImageReadUrlDto> CreateReadUrlAsync(
        string objectKey,
        CancellationToken cancellationToken)
    {
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(_options.ReadExpirationMinutes);

        var request = new GetPreSignedUrlRequest
        {
            BucketName = _options.BucketName,
            Key = objectKey,
            Verb = HttpVerb.GET,
            Expires = expiresAtUtc
        };

        var url = await _s3.GetPreSignedURLAsync(request);

        return new ImageReadUrlDto
        {
            Url = url,
            ExpiresAtUtc = expiresAtUtc
        };
    }

    private static string GetExtensionFromContentType(string contentType)
    {
        return contentType.Trim().ToLowerInvariant() switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",
            _ => throw new InvalidOperationException("Unsupported content type.")
        };
    }

    private static string SanitizePathSegment(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "unknown";
        }

        var invalidChars = Path.GetInvalidFileNameChars();
        var cleaned = new string(value
            .Trim()
            .Select(ch => invalidChars.Contains(ch) ? '-' : ch)
            .ToArray());

        return cleaned.Replace('/', '-').Replace('\\', '-');
    }
}