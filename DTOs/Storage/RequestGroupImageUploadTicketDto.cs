namespace Goal2026API.DTOs.Storage;

public sealed class RequestGroupImageUploadTicketDto
{
    public int? GroupId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
}