namespace Goal2026API.Api.Services;

public interface IGroupInvitationEmailService
{
    Task SendInvitationAsync(
        string toEmail,
        string groupName,
        string invitedByName,
        string acceptUrl,
        DateTime expiresAtUtc,
        CancellationToken cancellationToken = default);
}