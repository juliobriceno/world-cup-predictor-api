namespace Goal2026API.Api.Options;

public sealed class SendGridOptions
{
    public const string SectionName = "SendGrid";

    public string ApiKey { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public string? TemplateId { get; set; } // Default / English
    public string? TemplateIdEn { get; set; }
    public string? TemplateIdEs { get; set; }
    public string InvitationTemplateId { get; set; } = string.Empty;
    public string? InvitationTemplateIdEs { get; set; }
    public string? InvitationTemplateIdEn { get; set; }

}