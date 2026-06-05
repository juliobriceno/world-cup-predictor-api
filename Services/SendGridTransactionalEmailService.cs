using Goal2026API.Api.Options;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Goal2026API.Api.Services;

public sealed class SendGridTransactionalEmailService : ITransactionalEmailService
{
    private readonly SendGridOptions _options;
    private readonly ILogger<SendGridTransactionalEmailService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SendGridTransactionalEmailService(
        IOptions<SendGridOptions> options,
        ILogger<SendGridTransactionalEmailService> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _options = options.Value;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task SendTemplateEmailAsync(
        string toEmail,
        string templateId,
        object templateData,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
            throw new InvalidOperationException("SendGrid:ApiKey is missing.");

        if (string.IsNullOrWhiteSpace(_options.FromEmail))
            throw new InvalidOperationException("SendGrid:FromEmail is missing.");

        if (string.IsNullOrWhiteSpace(templateId))
            throw new InvalidOperationException("TemplateId is required.");

        var normalizedEmail = NormalizeEmail(toEmail);

        var client = new SendGridClient(_options.ApiKey);

        var message = new SendGridMessage
        {
            From = new EmailAddress(_options.FromEmail, _options.FromName),
            TemplateId = templateId
        };

        message.AddTo(new EmailAddress(normalizedEmail));
        message.SetTemplateData(templateData);

        var response = await client.SendEmailAsync(message, cancellationToken);

        if ((int)response.StatusCode >= 400)
        {
            var body = await response.Body.ReadAsStringAsync(cancellationToken);

            _logger.LogError(
                "SendGrid email send failed. StatusCode: {StatusCode}. Response: {ResponseBody}",
                response.StatusCode,
                body);

            throw new InvalidOperationException("Failed to send transactional email.");
        }
    }

    public async Task SendMagicLinkEmailAsync(
        string email,
        string magicLink,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
            throw new InvalidOperationException("SendGrid:ApiKey is missing.");

        if (string.IsNullOrWhiteSpace(_options.FromEmail))
            throw new InvalidOperationException("SendGrid:FromEmail is missing.");

        var templateId = GetMagicLinkTemplateId();

        if (string.IsNullOrWhiteSpace(templateId))
            throw new InvalidOperationException("SendGrid magic link TemplateId is missing.");

        var normalizedEmail = NormalizeEmail(email);

        var client = new SendGridClient(_options.ApiKey);

        var message = new SendGridMessage
        {
            From = new EmailAddress(_options.FromEmail, _options.FromName),
            TemplateId = templateId
        };

        message.AddTo(new EmailAddress(normalizedEmail));
        message.SetTemplateData(new
        {
            appName = _options.FromName,
            magicLink = magicLink,
            email = normalizedEmail
        });

        var response = await client.SendEmailAsync(message, cancellationToken);

        if ((int)response.StatusCode >= 400)
        {
            var body = await response.Body.ReadAsStringAsync(cancellationToken);

            _logger.LogError(
                "SendGrid email send failed. StatusCode: {StatusCode}. Response: {ResponseBody}",
                response.StatusCode,
                body);

            throw new InvalidOperationException("Failed to send transactional email.");
        }
    }

    private string? GetMagicLinkTemplateId()
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
                return FirstNotEmpty(_options.TemplateIdEs, _options.TemplateId);
            }

            if (language.StartsWith("en"))
            {
                return FirstNotEmpty(_options.TemplateIdEn, _options.TemplateId);
            }
        }

        return FirstNotEmpty(
            _options.TemplateId,
            _options.TemplateIdEn,
            _options.TemplateIdEs);
    }

    private static string? FirstNotEmpty(params string?[] values)
    {
        return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
    }

    private static string NormalizeEmail(string? email)
    {
        return (email ?? string.Empty).Trim().ToLowerInvariant();
    }
}