namespace RecommendationService.Api.Contracts;

public sealed record RecordViewRequest(Guid ProductId);

public sealed record RecommendedProductResponse(
    Guid ProductId,
    string Name,
    string PrimaryImageUrl,
    decimal EffectivePrice,
    double AverageRating,
    bool InStock);
