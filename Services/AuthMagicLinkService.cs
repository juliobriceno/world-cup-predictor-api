namespace Goal2026API.Api.Services;

public sealed class AuthMagicLinkService : IAuthMagicLinkService
{
    private readonly IFirebaseMagicLinkService _firebaseMagicLinkService;
    private readonly ITransactionalEmailService _transactionalEmailService;
    private readonly ILogger<AuthMagicLinkService> _logger;

    public AuthMagicLinkService(
        IFirebaseMagicLinkService firebaseMagicLinkService,
        ITransactionalEmailService transactionalEmailService,
        ILogger<AuthMagicLinkService> logger)
    {
        _firebaseMagicLinkService = firebaseMagicLinkService;
        _transactionalEmailService = transactionalEmailService;
        _logger = logger;
    }

    public async Task SendMagicLinkAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(email);

        try
        {
            var link = await _firebaseMagicLinkService.GenerateSignInLinkAsync(normalizedEmail, cancellationToken);
            await _transactionalEmailService.SendMagicLinkEmailAsync(normalizedEmail, link, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while generating or sending magic link for email {Email}.", normalizedEmail);
            throw;
        }
    }

    private static string NormalizeEmail(string? email)
    {
        return (email ?? string.Empty).Trim().ToLowerInvariant();
    }
}