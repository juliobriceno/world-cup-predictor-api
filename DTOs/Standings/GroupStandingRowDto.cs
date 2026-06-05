namespace Goal2026API.Api.DTOs;

public sealed class GroupStandingRowDto
{
    public int Position { get; set; }
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Nickname { get; set; }
    public int Points { get; set; }
    public string? PhotoUrl { get; set; }

}