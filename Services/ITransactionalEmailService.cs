namespace Goal2026API.Api.Services;

public interface ITransactionalEmailService
{
    Task SendMagicLinkEmailAsync(string email, string magicLink, CancellationToken cancellationToken = default);

    Task SendTemplateEmailAsync(
        string toEmail,
        string templateId,
        object templateData,
        CancellationToken cancellationToken = default);

}

