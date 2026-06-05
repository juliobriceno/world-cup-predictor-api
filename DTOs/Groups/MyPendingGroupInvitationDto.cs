namespace Goal2026API.Api.DTOs.Groups;

public sealed class MyPendingGroupInvitationDto
{
    public int InvitationId { get; set; }
    public int GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string InvitedEmail { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public bool IsExpired { get; set; }
}