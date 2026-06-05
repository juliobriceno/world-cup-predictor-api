using System.ComponentModel.DataAnnotations;

namespace Goal2026API.DTOs.Groups;

public sealed class CreateGroupDto
{
    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = null!;

    [Required]
    public GroupScoringRuleDto Scoring { get; set; } = null!;

    public string? ImageKey { get; set; }
    public string? ImageContentType { get; set; }

    public string? TimeZoneId { get; set; }

}