namespace Goal2026API.Api.Entities;

public sealed class User
{
    public int Id { get; set; }
    public string FirebaseUid { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Nickname { get; set; }

    public string? PhotoKey { get; set; }
    public string? PhotoContentType { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public string PreferredLanguage { get; set; } = "en";
    public ICollection<UserMatchSimulation> UserMatchSimulations { get; set; } = new List<UserMatchSimulation>();

}