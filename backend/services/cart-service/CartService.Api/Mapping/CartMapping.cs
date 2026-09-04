using CartService.Api.Contracts;
using CartService.Api.Domain;

namespace CartService.Api.Mapping;

public static class CartMapping
{
    public static CartResponse ToResponse(this Cart cart)
    {
        var items = cart.Items
            .OrderBy(i => i.AddedAtUtc)
            .Select(i => new CartItemResponse(
                i.Id,
                i.ProductId,
                i.ProductName,
                i.ProductImageUrl,
                i.UnitPrice,
                i.Quantity,
                i.UnitPrice * i.Quantity))
            .ToList();

        return new CartResponse(
            cart.Id,
            items,
            items.Sum(i => i.LineTotal),
            items.Sum(i => i.Quantity));
    }
}
