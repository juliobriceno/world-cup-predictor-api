using Goal2026API.Api.Options;
using Goal2026API.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

public sealed class GroupInvitationEmailService : IGroupInvitationEmailService
{
    private readonly ITransactionalEmailService _emailService;
    private readonly SendGridOptions _options;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GroupInvitationEmailService(
        ITransactionalEmailService emailService,
        IOptions<SendGridOptions> options,
        IHttpContextAccessor httpContextAccessor)
    {
        _emailService = emailService;
        _options = options.Value;
        _httpContextAccessor = httpContextAccessor;
    }

    private string? GetInvitationTemplateId()
    {
        var language = _httpContextAccessor.HttpContext?
            .Request
            .Headers["Accept-Language"]
            .ToString()
            .Trim()
            .ToLowerInvariant();

        if (!string.IsNullOrWhiteSpace(language))
        {
            if (language.StartsWith("es"))
            {
                return FirstNotEmpty(
                    _options.InvitationTemplateIdEs,
                    _options.InvitationTemplateId);
            }

            if (language.StartsWith("en"))
            {
                return FirstNotEmpty(
                    _options.InvitationTemplateIdEn,
                    _options.InvitationTemplateId);
            }
        }

        return FirstNotEmpty(
            _options.InvitationTemplateId,
            _options.InvitationTemplateIdEn,
            _options.InvitationTemplateIdEs);
    }

    private static string? FirstNotEmpty(params string?[] values)
    {
        return values.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
    }

    public async Task SendInvitationAsync(
        string toEmail,
        string groupName,
        string invitedByName,
        string acceptUrl,
        DateTime expiresAtUtc,
        CancellationToken cancellationToken = default)
    {
        var templateId = GetInvitationTemplateId();

        if (string.IsNullOrWhiteSpace(templateId))
        {
            throw new InvalidOperationException(
                "SendGrid invitation TemplateId is missing.");
        }

        var templateData = new
        {
            groupName,
            invitedByName,
            acceptUrl,
            expiresAt = expiresAtUtc.ToString("yyyy-MM-dd HH:mm")
        };

        await _emailService.SendTemplateEmailAsync(
            toEmail,
            templateId,
            templateData,
            cancellationToken);
    }
}