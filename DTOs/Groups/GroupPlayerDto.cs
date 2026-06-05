namespace Goal2026API.DTOs.Groups;

public sealed class GroupPlayerDto
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Nickname { get; set; }
    public string? PhotoUrl { get; set; }

    public DateTime JoinedAtUtc { get; set; }

    public bool IsEnabled { get; set; }
    public bool IsOwner { get; set; }
    public bool CanChangeStatus { get; set; }
}