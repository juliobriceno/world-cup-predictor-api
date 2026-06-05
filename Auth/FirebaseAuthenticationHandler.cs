using System.Security.Claims;
using System.Text.Encodings.Web;
using Goal2026API.Api.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Goal2026API.Api.Auth;

public sealed class FirebaseAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IFirebaseTokenService _firebaseTokenService;

    public FirebaseAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IFirebaseTokenService firebaseTokenService)
        : base(options, logger, encoder)
    {
        _firebaseTokenService = firebaseTokenService;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authHeader = Request.Headers.Authorization.ToString();

        if (string.IsNullOrWhiteSpace(authHeader) ||
            !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return AuthenticateResult.NoResult();
        }

        var token = authHeader["Bearer ".Length..].Trim();

        if (string.IsNullOrWhiteSpace(token))
        {
            return AuthenticateResult.Fail("Missing Firebase ID token.");
        }

        try
        {
            var tokenInfo = await _firebaseTokenService.VerifyIdTokenAsync(token, Context.RequestAborted);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, tokenInfo.Uid),
                new("uid", tokenInfo.Uid),
                new(ClaimTypes.Email, tokenInfo.Email),
                new("email", tokenInfo.Email),
                new("email_verified", tokenInfo.EmailVerified.ToString().ToLowerInvariant())
            };

            if (!string.IsNullOrWhiteSpace(tokenInfo.Name))
            {
                claims.Add(new Claim(ClaimTypes.Name, tokenInfo.Name));
                claims.Add(new Claim("name", tokenInfo.Name));
            }

            if (!string.IsNullOrWhiteSpace(tokenInfo.Picture))
            {
                claims.Add(new Claim("picture", tokenInfo.Picture));
            }

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
        catch (UnauthorizedAccessException ex)
        {
            return AuthenticateResult.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unexpected authentication error.");
            return AuthenticateResult.Fail("Authentication failed.");
        }
    }
}