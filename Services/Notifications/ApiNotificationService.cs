using Goal2026API.Api.Data;
using Goal2026API.Entities.FirebasePushNotifications;
using Microsoft.EntityFrameworkCore;

namespace Goal2026API.Api.Services;

public sealed class ApiNotificationService
{
    private readonly AppDbContext _db;

    public ApiNotificationService(AppDbContext db)
    {
        _db = db;
    }

    public async Task CreateAsync(
        int userId,
        string type,
        string title,
        string body,
        List<string> channels,
        string? deduplicationKey,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var notification = new Notification
        {
            UserId = userId,
            Type = type,
            Title = title,
            Body = body,
            DeduplicationKey = deduplicationKey,
            CreatedAtUtc = now
        };

        if (!string.IsNullOrWhiteSpace(deduplicationKey))
        {
            var existingNotification = await _db.Notifications
                .FirstOrDefaultAsync(x =>
                    x.UserId == userId &&
                    x.DeduplicationKey == deduplicationKey,
                    cancellationToken);

            if (existingNotification != null)
                return;
        }

        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync(cancellationToken);

        foreach (var channel in channels.Distinct())
        {
            _db.NotificationDeliveries.Add(new NotificationDelivery
            {
                NotificationId = notification.Id,
                UserId = userId,
                Channel = channel,
                Status = "Pending",
                RetryCount = 0,
                MaxRetries = 3,
                CreatedAtUtc = now
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}