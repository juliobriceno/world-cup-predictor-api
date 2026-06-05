namespace Goal2026API.Api.Services;

public interface IInvitationTokenService
{
    string GenerateToken();
    string ComputeHash(string token);
}