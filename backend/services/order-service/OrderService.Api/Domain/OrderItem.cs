namespace OrderService.Api.Domain;

/// <summary>
/// Line item snapshot taken from Cart Service at checkout, so the order's price/name/image never
/// change even if the product is later repriced or removed from the catalog.
/// </summary>
public sealed class OrderItem
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required Guid OrderId { get; init; }
    public Order? Order { get; init; }

    public required Guid ProductId { get; init; }
    public required string ProductName { get; set; }
    public required string ProductImageUrl { get; set; }
    public required decimal UnitPrice { get; set; }
    public required int Quantity { get; set; }
}
