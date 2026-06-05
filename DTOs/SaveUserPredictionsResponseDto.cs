namespace Goal2026API.Api.DTOs;

public sealed class SaveUserPredictionsResponseDto
{
    public string Message { get; set; } = string.Empty;
    public int TotalMatches { get; set; }
    public int PredictedMatches { get; set; }
    public int PendingMatches { get; set; }
    public DateTime SavedAtUtc { get; set; }
}