namespace CatalogService.Api.Contracts;

public sealed record CategoryResponse(Guid Id, string Name, string Slug, string? IconUrl);

public sealed record ProductSummaryResponse(
    Guid Id,
    string Name,
    string Slug,
    decimal Price,
    decimal EffectivePrice,
    int? DiscountPercent,
    string PrimaryImageUrl,
    double AverageRating,
    int RatingCount,
    int SoldCount,
    bool InStock);

public sealed record ProductDetailResponse(
    Guid Id,
    string Name,
    string Slug,
    string Description,
    decimal Price,
    decimal EffectivePrice,
    int? DiscountPercent,
    string PrimaryImageUrl,
    IReadOnlyList<string> Images,
    double AverageRating,
    int RatingCount,
    int SoldCount,
    int StockQuantity,
    CategoryResponse Category,
    string? BrandName);

public sealed record UpsertProductRequest(
    string Name,
    string Description,
    decimal Price,
    decimal? DiscountPrice,
    string PrimaryImageUrl,
    Guid CategoryId,
    Guid? BrandId,
    int StockQuantity);
