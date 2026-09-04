using InventoryService.Api.Domain;

namespace InventoryService.Api.Contracts;

internal static class InventoryMapping
{
    public static StockReservationResponse ToResponse(this StockReservation reservation) =>
        new(reservation.Id, reservation.OrderId, reservation.ProductId, reservation.Quantity, reservation.Status.ToString());

    public static StockLevelResponse ToResponse(this StockLevel stock) =>
        new(stock.ProductId, stock.QuantityOnHand, stock.QuantityReserved, stock.QuantityAvailable);
}
