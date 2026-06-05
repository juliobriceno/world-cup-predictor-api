using FirebaseAdmin.Auth;

namespace Goal2026API.Api.Services;

public interface IFirebaseTokenService
{
    Task<FirebaseTokenInfo> VerifyIdTokenAsync(string idToken, CancellationToken cancellationToken = default);
}

public sealed class FirebaseTokenService : IFirebaseTokenService
{
    public async Task<FirebaseTokenInfo> VerifyIdTokenAsync(string idToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(idToken))
        {
            throw new UnauthorizedAccessException("Missing Firebase ID token.");
        }

        FirebaseToken decodedToken;
        try
        {
            decodedToken = await FirebaseAuth.DefaultInstance
                .VerifyIdTokenAsync(idToken)
                .WaitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new UnauthorizedAccessException("Invalid Firebase ID token.", ex);
        }

        var email = decodedToken.Claims.TryGetValue("email", out var emailValue)
            ? Convert.ToString(emailValue)
            : null;

        var emailVerified = TryGetBooleanClaim(decodedToken.Claims, "email_verified");

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new UnauthorizedAccessException("The Firebase token does not contain a valid email.");
        }

        if (!emailVerified)
        {
            throw new UnauthorizedAccessException("The Firebase email is not verified.");
        }

        var name = decodedToken.Claims.TryGetValue("name", out var nameValue)
            ? Convert.ToString(nameValue)
            : null;

        var picture = decodedToken.Claims.TryGetValue("picture", out var pictureValue)
            ? Convert.ToString(pictureValue)
            : null;

        return new FirebaseTokenInfo
        {
            Uid = decodedToken.Uid,
            Email = email,
            EmailVerified = emailVerified,
            Name = name,
            Picture = picture
        };
    }

    private static bool TryGetBooleanClaim(IReadOnlyDictionary<string, object> claims, string claimName)
    {
        if (!claims.TryGetValue(claimName, out var value) || value is null)
        {
            return false;
        }

        if (value is bool boolValue)
        {
            return boolValue;
        }

        return bool.TryParse(Convert.ToString(value), out var parsed) && parsed;
    }
}

public sealed class FirebaseTokenInfo
{
    public string Uid { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool EmailVerified { get; set; }
    public string? Name { get; set; }
    public string? Picture { get; set; }
}