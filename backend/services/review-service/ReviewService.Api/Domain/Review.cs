namespace ReviewService.Api.Domain;

/// <summary>One user's rating/comment for a product. Unique per (ProductId, UserId) — one review per purchase relationship.</summary>
public sealed class Review
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required Guid ProductId { get; init; }
    public required Guid UserId { get; init; }
    public required int Rating { get; init; }
    public string? Comment { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; } = DateTimeOffset.UtcNow;
}
