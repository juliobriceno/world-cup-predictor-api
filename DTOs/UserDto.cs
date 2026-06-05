namespace Goal2026API.Api.DTOs;

public sealed class UserDto
{
    public int Id { get; set; }
    public string FirebaseUid { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Nickname { get; set; }

    public string? PhotoKey { get; set; }
    public string? PhotoContentType { get; set; }
    public string? PhotoUrl { get; set; }

    public string PreferredLanguage { get; set; } = "en";

    public DateTime CreatedAtUtc { get; set; }
}