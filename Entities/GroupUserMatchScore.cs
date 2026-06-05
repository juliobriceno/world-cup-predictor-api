namespace Goal2026API.Api.Entities;

public sealed class GroupUserMatchScore
{
    public int Id { get; set; }

    public int GroupId { get; set; }
    public int UserId { get; set; }
    public int MatchId { get; set; }

    public int Points { get; set; }

    public int OutcomePoints { get; set; }
    public int ExactHomeGoalsPoints { get; set; }
    public int ExactAwayGoalsPoints { get; set; }
    public int CategoryPoints { get; set; }

    public int? PredictedHomeGoals { get; set; }
    public int? PredictedAwayGoals { get; set; }

    public int OfficialHomeGoals { get; set; }
    public int OfficialAwayGoals { get; set; }

    public DateTime CalculatedAtUtc { get; set; }
}