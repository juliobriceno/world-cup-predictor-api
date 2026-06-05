using Goal2026API.DTOs.UserSimulationsDto;

namespace Goal2026API.Services
{
    public interface IUserSimulationService
    {
        Task<SaveUserSimulationsResponseDto?> SaveMySimulationsAsync(
            string firebaseUid,
            SaveUserSimulationsDto dto,
            CancellationToken cancellationToken);

        Task<GetUserSimulationsResponseDto?> GetMySimulationsAsync(
            string firebaseUid,
            CancellationToken cancellationToken);
    }
}
