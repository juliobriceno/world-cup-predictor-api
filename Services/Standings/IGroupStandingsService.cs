using Goal2026API.Api.DTOs;
using Goal2026API.Api.DTOs.Groups;

namespace Goal2026API.Api.Services;

public interface IGroupStandingsService
{
    Task<GroupStandingsResponseDto?> GetGroupStandingsAsync(
        int groupId,
        string firebaseUid,
        GroupStandingsMode mode,
        CancellationToken cancellationToken);

    Task<GroupDashboardResponseDto?> GetGroupDashboardAsync(
        int groupId,
        string firebaseUid,
        GroupStandingsMode mode,
        CancellationToken cancellationToken);

}