namespace Goal2026API.Api.Services
{
    public interface IRecaptchaService
    {
        Task<bool> IsValidAsync(
            string token,
            string expectedAction,
            CancellationToken cancellationToken);
    }
}