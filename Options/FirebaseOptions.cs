namespace Goal2026API.Api.Options;

public sealed class FirebaseOptions
{
    public const string SectionName = "Firebase";

    public string ServiceAccountPath { get; set; } = string.Empty;
    public string ServiceAccountJson { get; set; } = string.Empty;
}