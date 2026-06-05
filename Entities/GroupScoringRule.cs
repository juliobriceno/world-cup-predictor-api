namespace Goal2026API.Api.Entities;

public sealed class GroupScoringRule
{
    public int GroupId { get; set; }
    public Group Group { get; set; } = null!;

    public bool EnableOutcomeRule { get; set; }
    public int? OutcomePoints { get; set; }

    public bool EnableExactScoreRule { get; set; }
    public int? ExactHomeGoalsPoints { get; set; }
    public int? ExactAwayGoalsPoints { get; set; }
    public bool RequireBothExactScores { get; set; }

    public bool EnableGoalDifferenceRule { get; set; }
    public int? ClosedMatchPoints { get; set; }
    public int? ComfortableWinPoints { get; set; }
    public int? BlowoutPoints { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public int CreatedByUserId { get; set; }
    public User CreatedByUser { get; set; } = null!;

    public DateTime UpdatedAtUtc { get; set; }
    public int UpdatedByUserId { get; set; }
    public User UpdatedByUser { get; set; } = null!;
}