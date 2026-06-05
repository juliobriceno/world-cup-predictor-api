namespace Goal2026API.Api.Entities;

public sealed class UserMatchSimulation
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int MatchId { get; set; }

    public int? SimulatedHomeGoals { get; set; }
    public int? SimulatedAwayGoals { get; set; }
    public bool HasSimulation { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}