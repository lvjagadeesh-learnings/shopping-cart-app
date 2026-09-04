namespace RecommendationService.Api.Domain;

/// <summary>One row per purchased line item — recorded from OrderPlacedEvent. Weighted higher than views for "trending".</summary>
public sealed class ProductPurchase
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required Guid UserId { get; init; }
    public required Guid ProductId { get; init; }
    public required int Quantity { get; init; }
    public DateTimeOffset PurchasedAtUtc { get; init; } = DateTimeOffset.UtcNow;
}
