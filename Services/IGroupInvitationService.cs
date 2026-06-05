using Goal2026API.Api.DTOs.Groups;

namespace Goal2026API.Api.Services;

public interface IGroupInvitationService
{
    Task<GroupInvitationDto> CreateAsync(
        string firebaseUid,
        int groupId,
        string email,
        CancellationToken cancellationToken = default);

    Task<List<MyPendingGroupInvitationDto>> GetMyPendingInvitationsAsync(
        string firebaseUid,
        CancellationToken cancellationToken = default);

    Task<ResolveGroupInvitationResponseDto> ResolveAsync(
        string? firebaseUid,
        string token,
        CancellationToken cancellationToken = default);

    Task AcceptAsync(
        string firebaseUid,
        string token,
        CancellationToken cancellationToken = default);

    Task DeclineAsync(
        string firebaseUid,
        string token,
        CancellationToken cancellationToken = default);

    Task AcceptByInvitationIdAsync(
        string firebaseUid,
        int invitationId,
        CancellationToken cancellationToken = default);

    Task DeclineByInvitationIdAsync(
        string firebaseUid,
        int invitationId,
        CancellationToken cancellationToken = default);
}