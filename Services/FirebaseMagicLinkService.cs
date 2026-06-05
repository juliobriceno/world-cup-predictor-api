using FirebaseAdmin.Auth;
using Goal2026API.Api.Options;
using Microsoft.Extensions.Options;

namespace Goal2026API.Api.Services;

public sealed class FirebaseMagicLinkService : IFirebaseMagicLinkService
{
    private readonly FirebaseMagicLinkOptions _options;

    public FirebaseMagicLinkService(IOptions<FirebaseMagicLinkOptions> options)
    {
        _options = options.Value;
    }

    public async Task<string> GenerateSignInLinkAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(email);

        if (string.IsNullOrWhiteSpace(normalizedEmail))
        {
            throw new ArgumentException("Email is required.", nameof(email));
        }

        if (string.IsNullOrWhiteSpace(_options.ContinueUrl))
        {
            throw new InvalidOperationException("FirebaseMagicLink:ContinueUrl is missing.");
        }

        var actionCodeSettings = new ActionCodeSettings
        {
            Url = _options.ContinueUrl,
            HandleCodeInApp = _options.HandleCodeInApp
        };

        var link = await FirebaseAuth.DefaultInstance
            .GenerateSignInWithEmailLinkAsync(normalizedEmail, actionCodeSettings)
            .WaitAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(link))
        {
            throw new InvalidOperationException("Firebase did not generate a valid sign-in link.");
        }

        return link;
    }

    private static string NormalizeEmail(string? email)
    {
        return (email ?? string.Empty).Trim().ToLowerInvariant();
    }
}