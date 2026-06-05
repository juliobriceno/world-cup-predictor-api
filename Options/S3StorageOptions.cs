namespace Goal2026API.Api.Options;

public sealed class S3StorageOptions
{
    public const string SectionName = "S3Storage";

    public string BucketName { get; set; } = string.Empty;
    public string Region { get; set; } = "us-east-2";
    public string AwsProfile { get; set; } = "default";

    public int UploadExpirationMinutes { get; set; } = 5;
    public int ReadExpirationMinutes { get; set; } = 10;
    public long MaxFileSizeBytes { get; set; } = 3145728;

    public string[] AllowedContentTypes { get; set; } =
    [
        "image/jpeg",
        "image/png",
        "image/webp"
    ];
}