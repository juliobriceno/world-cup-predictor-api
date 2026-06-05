namespace Goal2026API.DTOs.Storage;

public sealed class ImageUploadTicketDto
{
    public string Url { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public Dictionary<string, string> Fields { get; set; } = new();
}