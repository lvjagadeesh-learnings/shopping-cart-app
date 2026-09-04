using BuildingBlocks.Events;
using RecommendationService.Api.Data;
using RecommendationService.Api.Domain;

namespace RecommendationService.Api.EventHandlers;

/// <summary>Persists one ProductPurchase row per order line item, feeding the "trending" ranking.</summary>
public sealed class OrderPlacedRecommendationHandler(RecommendationDbContext db) : IEventHandler<OrderPlacedEvent>
{
    public async Task HandleAsync(OrderPlacedEvent @event, CancellationToken cancellationToken)
    {
        foreach (var item in @event.Items)
        {
            db.ProductPurchases.Add(new ProductPurchase
            {
                UserId = @event.UserId,
                ProductId = item.ProductId,
                Quantity = item.Quantity
            });
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
