namespace Goal2026API.DTOs.Groups;

public sealed class GroupDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;

    public int OwnerUserId { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
    public bool CanModifyRules { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    public string? ImageKey { get; set; }
    public string? ImageContentType { get; set; }
    public DateTime? ImageUpdatedAtUtc { get; set; }
    public string? ImageUrl { get; set; }

    public GroupScoringRuleDto Scoring { get; set; } = new();

    public string? TimeZoneId { get; set; }

}