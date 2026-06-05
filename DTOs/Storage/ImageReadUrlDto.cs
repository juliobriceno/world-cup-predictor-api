namespace Goal2026API.DTOs.Storage;

public sealed class ImageReadUrlDto
{
    public string Url { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
}