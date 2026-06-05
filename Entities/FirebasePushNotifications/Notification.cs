using Goal2026API.Api.Entities;

namespace Goal2026API.Entities.FirebasePushNotifications
{
    public class Notification
    {
        public long Id { get; set; }

        public int UserId { get; set; }

        public string Type { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;

        public string? DataJson { get; set; }

        public string? DeduplicationKey { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public User? User { get; set; }

        public ICollection<NotificationDelivery> Deliveries { get; set; } = new List<NotificationDelivery>();
    }
}
