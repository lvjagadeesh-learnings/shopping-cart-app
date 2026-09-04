namespace CartService.Api.Domain;

/// <summary>
/// Product name/image/price are denormalized snapshots taken from Catalog Service at the moment
/// the item was added, so the cart renders instantly without a live Catalog call and so historical
/// cart totals aren't silently altered by later price changes. <see cref="UnitPrice"/> is refreshed
/// on quantity update calls so the shopper always sees a reasonably current price before checkout.
/// </summary>
public sealed class CartItem
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required Guid CartId { get; init; }
    public Cart? Cart { get; init; }

    public required Guid ProductId { get; init; }
    public required string ProductName { get; set; }
    public required string ProductImageUrl { get; set; }
    public required decimal UnitPrice { get; set; }

    public required int Quantity { get; set; }
    public DateTimeOffset AddedAtUtc { get; init; } = DateTimeOffset.UtcNow;
}
