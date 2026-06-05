namespace Goal2026API.DTOs.Groups;

public sealed class GroupScoringRuleDto
{
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
}