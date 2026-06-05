using System.ComponentModel.DataAnnotations;

namespace Goal2026API.Api.Contracts.Auth;

public sealed class SendMagicLinkRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string ContinueUrl { get; set; } = string.Empty;

    public string RecaptchaToken { get; set; } = string.Empty;

}