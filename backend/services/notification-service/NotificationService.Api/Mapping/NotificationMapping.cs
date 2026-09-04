using NotificationService.Api.Contracts;
using NotificationService.Api.Domain;

namespace NotificationService.Api.Mapping;

public static class NotificationMapping
{
    public static NotificationResponse ToResponse(this NotificationLog log) => new(
        log.Id,
        log.EventType,
        log.Title,
        log.Message,
        log.IsRead,
        log.CreatedAtUtc);
}
