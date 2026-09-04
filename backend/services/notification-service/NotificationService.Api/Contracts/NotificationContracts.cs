namespace NotificationService.Api.Contracts;

public sealed record NotificationResponse(
    Guid Id,
    string EventType,
    string Title,
    string Message,
    bool IsRead,
    DateTimeOffset CreatedAtUtc);
