namespace Goal2026API.Api.DTOs;

public sealed class RegisterDeviceTokenRequest
{
    public string Token { get; set; } = string.Empty;

    public string Platform { get; set; } = "Web";

    public string? DeviceName { get; set; }

    public string? AppVersion { get; set; }
}