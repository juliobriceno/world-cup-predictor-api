namespace Goal2026API.Api.DTOs.Groups;

public sealed class ResolveGroupInvitationResponseDto
{
    public int GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string InvitedEmail { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public bool IsExpired { get; set; }

    public bool RequiresAuthentication { get; set; }
    public bool EmailMatchesAuthenticatedUser { get; set; }
    public bool IsAlreadyMember { get; set; }
}