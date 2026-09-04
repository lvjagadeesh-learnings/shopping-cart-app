namespace NotificationService.Api.Domain;

/// <summary>In-app + simulated-email log entry, one row per notification sent to a user.</summary>
public sealed class NotificationLog
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required Guid UserId { get; init; }
    public required string EventType { get; init; }
    public required string Title { get; init; }
    public required string Message { get; init; }
    public bool IsRead { get; set; }
    public DateTimeOffset CreatedAtUtc { get; init; } = DateTimeOffset.UtcNow;
}
