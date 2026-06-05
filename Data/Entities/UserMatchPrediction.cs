using Goal2026API.Api.Entities;

namespace Goal2026API.Api.Data.Entities;

public sealed class UserMatchPrediction
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int MatchId { get; set; }
    public Match Match { get; set; } = null!;

    public int? PredictedHomeGoals { get; set; }
    public int? PredictedAwayGoals { get; set; }

    public bool HasPrediction { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}