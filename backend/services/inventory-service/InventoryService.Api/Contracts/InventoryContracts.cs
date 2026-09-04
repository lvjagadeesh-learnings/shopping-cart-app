namespace InventoryService.Api.Contracts;

public sealed record ReserveStockItem(Guid ProductId, int Quantity);

public sealed record ReserveStockRequest(Guid OrderId, IReadOnlyList<ReserveStockItem> Items);

public sealed record StockReservationResponse(Guid Id, Guid OrderId, Guid ProductId, int Quantity, string Status);

public sealed record StockLevelResponse(Guid ProductId, int QuantityOnHand, int QuantityReserved, int QuantityAvailable);

public sealed record AdjustStockRequest(int QuantityOnHand);
