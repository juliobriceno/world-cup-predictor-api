using Goal2026API.Api.Entities;

namespace Goal2026API.Api.Data.Entities;

public sealed class Match
{
    public int Id { get; set; }
    public string GroupCode { get; set; } = string.Empty;
    public int MatchNumber { get; set; }
    public string HomeTeam { get; set; } = string.Empty;
    public string AwayTeam { get; set; } = string.Empty;
    public string HomeFlag { get; set; } = string.Empty;
    public string AwayFlag { get; set; } = string.Empty;
    public string Stadium { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public DateTime MatchDateUtc { get; set; }
    public string StageCode { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsFinished { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    public int? HomeTeamGoals { get; set; }
    public int? AwayTeamGoals { get; set; }

    public ICollection<UserMatchPrediction> UserMatchPredictions { get; set; } = new List<UserMatchPrediction>();
    public ICollection<UserMatchSimulation> UserMatchSimulations { get; set; } = new List<UserMatchSimulation>();
}