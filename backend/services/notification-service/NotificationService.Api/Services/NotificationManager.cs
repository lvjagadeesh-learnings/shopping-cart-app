using BuildingBlocks.Common;
using Microsoft.EntityFrameworkCore;
using NotificationService.Api.Contracts;
using NotificationService.Api.Data;
using NotificationService.Api.Domain;
using NotificationService.Api.Mapping;

namespace NotificationService.Api.Services;

public sealed class NotificationManager(NotificationDbContext db)
{
    public async Task LogAsync(Guid userId, string eventType, string title, string message, CancellationToken ct)
    {
        db.NotificationLogs.Add(new NotificationLog
        {
            UserId = userId,
            EventType = eventType,
            Title = title,
            Message = message
        });
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<NotificationResponse>> GetForUserAsync(Guid userId, CancellationToken ct) =>
        await db.NotificationLogs
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAtUtc)
            .Select(n => n.ToResponse())
            .ToListAsync(ct);

    public async Task MarkAsReadAsync(Guid userId, Guid notificationId, CancellationToken ct)
    {
        var log = await db.NotificationLogs.SingleOrDefaultAsync(n => n.Id == notificationId, ct)
            ?? throw new NotFoundException("Notification not found.");

        if (log.UserId != userId)
        {
            throw new NotFoundException("Notification not found.");
        }

        log.IsRead = true;
        await db.SaveChangesAsync(ct);
    }
}
