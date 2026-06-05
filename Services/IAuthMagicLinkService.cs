namespace Goal2026API.Api.Services;

public interface IAuthMagicLinkService
{
    Task SendMagicLinkAsync(string email, CancellationToken cancellationToken = default);
}