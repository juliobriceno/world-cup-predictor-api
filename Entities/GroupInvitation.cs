namespace Goal2026API.Api.Entities;

public sealed class GroupInvitation
{
    public int Id { get; set; }

    public int GroupId { get; set; }

    public int? InvitedUserId { get; set; }

    public string InvitedEmail { get; set; } = null!;
    public string InvitedEmailNormalized { get; set; } = null!;

    public string TokenHash { get; set; } = null!;
    public DateTime ExpiresAtUtc { get; set; }

    public int CreatedByUserId { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAtUtc { get; set; }
    public DateTime? RespondedAtUtc { get; set; }

    public int? AcceptedByUserId { get; set; }
    public int? DeclinedByUserId { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime UpdatedAtUtc { get; set; }
    public int UpdatedByUserId { get; set; }

    public Group Group { get; set; } = null!;
    public User? InvitedUser { get; set; }
    public User CreatedByUser { get; set; } = null!;
    public User? AcceptedByUser { get; set; }
    public User? DeclinedByUser { get; set; }
    public User UpdatedByUser { get; set; } = null!;
}