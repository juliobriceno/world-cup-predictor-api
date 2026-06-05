using Goal2026API.Api.DTOs;

namespace Goal2026API.Api.Services;

public interface IUserPredictionService
{
    Task<SaveUserPredictionsResponseDto?> SaveMyPredictionsAsync(
        string firebaseUid,
        SaveUserPredictionsDto dto,
        CancellationToken cancellationToken);

    Task<GetUserPredictionsResponseDto?> GetMyPredictionsAsync(
        string firebaseUid,
        CancellationToken cancellationToken);

}
