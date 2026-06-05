using Goal2026API.Api.Contracts.Auth;
using Goal2026API.Api.Contracts.Common;
using Goal2026API.Api.DTOs;
using Goal2026API.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Goal2026API.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IFirebaseTokenService _firebaseTokenService;
    private readonly IUserSyncService _userSyncService;
    private readonly IAuthMagicLinkService _authMagicLinkService;
    private readonly ILogger<AuthController> _logger;

    private readonly IRecaptchaService _recaptchaService;

    public AuthController(
        IFirebaseTokenService firebaseTokenService,
        IUserSyncService userSyncService,
        IAuthMagicLinkService authMagicLinkService,
        ILogger<AuthController> logger, IRecaptchaService recaptchaService)
    {
        _firebaseTokenService = firebaseTokenService;
        _userSyncService = userSyncService;
        _authMagicLinkService = authMagicLinkService;
        _logger = logger;
        _recaptchaService = recaptchaService;
    }

    [EnableRateLimiting("magic-link")]
    [HttpPost("send-magic-link")]
    [AllowAnonymous]
    public async Task<IActionResult> SendMagicLink(
        [FromBody] SendMagicLinkRequest request,
        CancellationToken cancellationToken)
    {
        var response = new GenericMessageResponse
        {
            Message = "If the email can receive sign-in links, the message has been sent."
        };

        if (!ModelState.IsValid)
        {
            return Ok(response);
        }

        var normalizedEmail = NormalizeEmail(request.Email);

        if (string.IsNullOrWhiteSpace(normalizedEmail))
        {
            return Ok(response);
        }

        try
        {
            var recaptchaIsValid = await _recaptchaService.IsValidAsync(
                request.RecaptchaToken,
                "send_magic_link",
                cancellationToken);

            if (!recaptchaIsValid)
            {
                _logger.LogWarning(
                    "Blocked send-magic-link by recaptcha. Email: {Email}",
                    normalizedEmail);

                return Ok(response);
            }

            await _authMagicLinkService.SendMagicLinkAsync(
                normalizedEmail,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while sending magic link.");

            return StatusCode(StatusCodes.Status500InternalServerError, new GenericMessageResponse
            {
                Message = "An unexpected error occurred."
            });
        }

        return Ok(response);
    }

    [HttpPost("sync-user")]
    [EnableRateLimiting("auth-sync")]
    public async Task<IActionResult> SyncUser(
        [FromBody] SyncUserRequestDto? dto,
        CancellationToken cancellationToken)
    {
        var authHeader = Request.Headers.Authorization.ToString();

        if (string.IsNullOrWhiteSpace(authHeader) ||
            !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return Unauthorized(new { message = "Authorization header with Bearer token is required." });
        }

        var token = authHeader["Bearer ".Length..].Trim();

        var tokenInfo = await _firebaseTokenService.VerifyIdTokenAsync(token, cancellationToken);

        var user = await _userSyncService.SyncUserAsync(
            tokenInfo,
            dto,
            cancellationToken);

        return Ok(user);
    }

    private static string NormalizeEmail(string? email)
    {
        return (email ?? string.Empty).Trim().ToLowerInvariant();
    }
}