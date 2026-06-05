using Goal2026API.Api.Contracts.Auth;
using Goal2026API.Api.Services;
using System.Net.Http.Json;

namespace Goal2026API.Api.Services
{
    public sealed class RecaptchaService : IRecaptchaService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RecaptchaService> _logger;

        public RecaptchaService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<RecaptchaService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> IsValidAsync(
            string token,
            string expectedAction,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return false;
            }

            var secretKey = _configuration["Recaptcha:SecretKey"];
            var minimumScore = _configuration.GetValue<decimal>("Recaptcha:MinimumScore", 0.5m);

            if (string.IsNullOrWhiteSpace(secretKey))
            {
                _logger.LogError("Recaptcha secret key is not configured.");
                return false;
            }

            using var form = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["secret"] = secretKey,
                ["response"] = token
            });

            using var response = await _httpClient.PostAsync(
                "https://www.google.com/recaptcha/api/siteverify",
                form,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Recaptcha verification HTTP error: {StatusCode}", response.StatusCode);
                return false;
            }

            var payload = await response.Content.ReadFromJsonAsync<RecaptchaVerifyResponse>(
                cancellationToken: cancellationToken);

            if (payload is null)
            {
                _logger.LogWarning("Recaptcha verification returned null payload.");
                return false;
            }

            if (!payload.Success)
            {
                _logger.LogWarning(
                    "Recaptcha failed. Errors: {Errors}",
                    payload.ErrorCodes is null ? "none" : string.Join(", ", payload.ErrorCodes));
                return false;
            }

            if (!string.Equals(payload.Action, expectedAction, StringComparison.Ordinal))
            {
                _logger.LogWarning(
                    "Recaptcha action mismatch. Expected {ExpectedAction}, got {ActualAction}",
                    expectedAction,
                    payload.Action);
                return false;
            }

            if (payload.Score < minimumScore)
            {
                _logger.LogWarning(
                    "Recaptcha score too low. Score {Score}, minimum {MinimumScore}",
                    payload.Score,
                    minimumScore);
                return false;
            }

            return true;
        }
    }
}