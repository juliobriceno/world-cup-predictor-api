namespace Goal2026API.Api.DTOs;

public sealed class SaveUserPredictionsDto
{
    public List<UserPredictionItemDto> Matches { get; set; } = new();
}