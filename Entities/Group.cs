using Goal2026API.Api.Entities;

namespace Goal2026API.Api.Entities;

public sealed class Group
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;

    public int OwnerUserId { get; set; }
    public User OwnerUser { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public int CreatedByUserId { get; set; }
    public User CreatedByUser { get; set; } = null!;

    public DateTime UpdatedAtUtc { get; set; }
    public int UpdatedByUserId { get; set; }
    public User UpdatedByUser { get; set; } = null!;

    public ICollection<GroupMember> Members { get; set; } = new List<GroupMember>();
    public GroupScoringRule? ScoringRule { get; set; }
    public ICollection<GroupInvitation> Invitations { get; set; } = new List<GroupInvitation>();

    public string? ImageKey { get; set; }
    public string? ImageContentType { get; set; }
    public DateTime? ImageUpdatedAtUtc { get; set; }

    public string TimeZoneId { get; set; } = "America/New_York";

}