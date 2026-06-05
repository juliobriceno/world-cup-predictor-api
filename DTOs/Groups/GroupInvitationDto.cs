namespace Goal2026API.Api.DTOs.Groups;

public sealed class GroupInvitationDto
{
    public int Id { get; set; }
    public int GroupId { get; set; }
    public string InvitedEmail { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime ExpiresAtUtc { get; set; }
}

public class CreateGroupInvitationsBulkDto
{
    public List<string> Emails { get; set; } = new();
}

public class BulkInvitationResultDto
{
    public int Total { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }

    public List<string> SuccessfulEmails { get; set; } = new();
    public List<BulkInvitationErrorDto> Errors { get; set; } = new();
}

public class BulkInvitationErrorDto
{
    public string Email { get; set; }
    public string Error { get; set; }
}