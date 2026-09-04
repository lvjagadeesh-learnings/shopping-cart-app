using OrderService.Api.Contracts;
using OrderService.Api.Domain;

namespace OrderService.Api.Mapping;

public static class OrderMapping
{
    // Items are passed explicitly (rather than read from order.Items) so checkout can build the
    // response before the order/items round-trip through the change tracker as separate DbSet adds.
    public static OrderResponse ToResponse(this Order order, IEnumerable<OrderItem> items)
    {
        var itemResponses = items
            .Select(i => new OrderItemResponse(
                i.Id,
                i.ProductId,
                i.ProductName,
                i.ProductImageUrl,
                i.UnitPrice,
                i.Quantity,
                i.UnitPrice * i.Quantity))
            .ToList();

        return new OrderResponse(order.Id, order.Status.ToString(), order.TotalAmount, itemResponses, order.CreatedAtUtc);
    }

    public static OrderSummaryResponse ToSummaryResponse(this Order order) =>
        new(order.Id, order.Status.ToString(), order.TotalAmount, order.Items.Sum(i => i.Quantity), order.CreatedAtUtc);
}
