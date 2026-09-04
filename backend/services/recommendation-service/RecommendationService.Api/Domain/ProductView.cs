namespace RecommendationService.Api.Domain;

/// <summary>One row per product view — used to compute "trending" alongside purchases.</summary>
public sealed class ProductView
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required Guid UserId { get; init; }
    public required Guid ProductId { get; init; }
    public DateTimeOffset ViewedAtUtc { get; init; } = DateTimeOffset.UtcNow;
}
