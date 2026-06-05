using Goal2026API.Api.Entities;

namespace Goal2026API.Entities.FirebasePushNotifications
{
    public class NotificationDelivery
    {
        public long Id { get; set; }

        public long NotificationId { get; set; }

        public int UserId { get; set; }

        public string Channel { get; set; } = string.Empty;
        // Push, Email

        public string Status { get; set; } = "Pending";
        // Pending, Processing, Sent, Failed

        public int RetryCount { get; set; }

        public int MaxRetries { get; set; } = 3;

        public DateTime? ScheduledAtUtc { get; set; }

        public DateTime? LockedAtUtc { get; set; }

        public string? LockedBy { get; set; }

        public DateTime? SentAtUtc { get; set; }

        public DateTime? FailedAtUtc { get; set; }

        public string? ErrorMessage { get; set; }

        public string? ProviderResponse { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public Notification Notification { get; set; } = null!;

        public User User { get; set; } = null!;
    }
}
