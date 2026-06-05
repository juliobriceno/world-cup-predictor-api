using Goal2026API.DTOs.Common;
using Goal2026API.DTOs.Groups;

namespace Goal2026API.Services;

public interface IGroupService
{
    Task<ServiceResult<GroupDto>> CreateGroupAsync(
        string firebaseUid,
        CreateGroupDto dto,
        CancellationToken cancellationToken);

    Task<GroupDto?> GetGroupByIdForUserAsync(
        string firebaseUid,
        int groupId,
        CancellationToken cancellationToken);

    Task<ServiceResult<GroupDto>> UpdateGroupAsync(
        string firebaseUid,
        int groupId,
        UpdateGroupDto dto,
        CancellationToken cancellationToken);

    Task<ServiceResult<bool>> DeleteGroupAsync(
        string firebaseUid,
        int groupId,
        CancellationToken cancellationToken);

    Task<List<MyGroupListItemDto>> GetMyGroupsAsync(
        string firebaseUid,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<List<GroupPlayerDto>>> GetGroupPlayersAsync(
        string firebaseUid,
        int groupId,
        CancellationToken cancellationToken);

    Task<ServiceResult<GroupPlayerDto>> UpdateGroupPlayerStatusAsync(
        string firebaseUid,
        int groupId,
        int userId,
        bool isEnabled,
        CancellationToken cancellationToken);

}