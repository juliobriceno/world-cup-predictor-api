namespace Goal2026API.Api.DTOs;

public sealed class UserPredictionItemDto
{
    public int MatchId { get; set; }
    public int? PredictedHomeGoals { get; set; }
    public int? PredictedAwayGoals { get; set; }
}

public sealed class UserPredictionItemResponseDto
{
    public int MatchId { get; set; }
    public int? PredictedHomeGoals { get; set; }
    public int? PredictedAwayGoals { get; set; }
    public bool HasPrediction { get; set; }
}

public sealed class GetUserPredictionsResponseDto
{
    public int TotalMatches { get; set; }
    public int PredictedMatches { get; set; }
    public int PendingMatches { get; set; }
    public List<UserPredictionItemResponseDto> Matches { get; set; } = new();
}