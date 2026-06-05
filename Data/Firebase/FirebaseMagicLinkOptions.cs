namespace Goal2026API.Api.Options;

public sealed class FirebaseMagicLinkOptions
{
    public const string SectionName = "FirebaseMagicLink";

    public string ContinueUrl { get; set; } = string.Empty;
    public bool HandleCodeInApp { get; set; } = true;
}