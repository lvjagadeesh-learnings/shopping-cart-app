namespace CartService.Api.Contracts;

public sealed record CartItemResponse(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string ProductImageUrl,
    decimal UnitPrice,
    int Quantity,
    decimal LineTotal);

public sealed record CartResponse(
    Guid Id,
    IReadOnlyList<CartItemResponse> Items,
    decimal Subtotal,
    int TotalItems);

public sealed record AddCartItemRequest(Guid ProductId, int Quantity);

public sealed record UpdateCartItemRequest(int Quantity);

/// <summary>Minimal product info needed by Cart Service, fetched from Catalog Service.</summary>
public sealed record ProductLookupResult(
    Guid Id,
    string Name,
    string PrimaryImageUrl,
    decimal EffectivePrice,
    bool InStock);
