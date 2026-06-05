using Goal2026API.Api.Data.Entities;
using Goal2026API.Api.Entities;

namespace Goal2026API.Api.Data.Entities
{

    public class UserDeviceToken
    {
        public long Id { get; set; }

        public int UserId { get; set; } // 👈 IMPORTANTE: int (no long)

        public string Token { get; set; } = string.Empty;

        public string Channel { get; set; } = "Push";

        public string Platform { get; set; } = "Web";

        public string? DeviceName { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? LastSeenAt { get; set; }

        public DateTime? LastSentAt { get; set; }

        public DateTime? InvalidatedAt { get; set; }

        public string? InvalidReason { get; set; }

        public User User { get; set; } = null!;

        public string? AppVersion { get; set; }

    }


}
