namespace OrderService.Api.Contracts;

public sealed record OrderItemResponse(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string ProductImageUrl,
    decimal UnitPrice,
    int Quantity,
    decimal LineTotal);

public sealed record OrderResponse(
    Guid Id,
    string Status,
    decimal TotalAmount,
    IReadOnlyList<OrderItemResponse> Items,
    DateTimeOffset CreatedAtUtc);

public sealed record OrderSummaryResponse(
    Guid Id,
    string Status,
    decimal TotalAmount,
    int TotalItems,
    DateTimeOffset CreatedAtUtc);

public sealed record UpdateOrderStatusRequest(string Status);

/// <summary>Mirrors CartService.Api.Contracts.CartResponse's JSON shape without a project reference.</summary>
public sealed record CartLookupItem(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string ProductImageUrl,
    decimal UnitPrice,
    int Quantity,
    decimal LineTotal);

public sealed record CartLookupResponse(Guid Id, IReadOnlyList<CartLookupItem> Items, decimal Subtotal, int TotalItems);

/// <summary>Mirrors InventoryService.Api.Contracts request shapes without a project reference.</summary>
public sealed record ReserveStockItem(Guid ProductId, int Quantity);

public sealed record ReserveStockRequest(Guid OrderId, IReadOnlyList<ReserveStockItem> Items);

/// <summary>Mirrors PaymentService.Api.Contracts shapes without a project reference.</summary>
public sealed record AuthorizePaymentRequest(Guid OrderId, decimal Amount, string? Currency);

public sealed record PaymentResult(Guid Id, Guid OrderId, decimal Amount, string Currency, string Status, string? FailureReason);
