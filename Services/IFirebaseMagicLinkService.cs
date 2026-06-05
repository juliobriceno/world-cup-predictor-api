namespace Goal2026API.Api.Services;

public interface IFirebaseMagicLinkService
{
    Task<string> GenerateSignInLinkAsync(string email, CancellationToken cancellationToken = default);
}