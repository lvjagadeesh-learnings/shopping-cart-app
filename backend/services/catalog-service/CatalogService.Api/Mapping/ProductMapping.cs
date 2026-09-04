using CatalogService.Api.Contracts;
using CatalogService.Api.Domain;

namespace CatalogService.Api.Mapping;

public static class ProductMapping
{
    public static ProductSummaryResponse ToSummary(this Product product) => new(
        product.Id,
        product.Name,
        product.Slug,
        product.Price,
        product.EffectivePrice,
        product.DiscountPercent,
        product.PrimaryImageUrl,
        product.AverageRating,
        product.RatingCount,
        product.SoldCount,
        product.StockQuantity > 0);

    public static ProductDetailResponse ToDetail(this Product product) => new(
        product.Id,
        product.Name,
        product.Slug,
        product.Description,
        product.Price,
        product.EffectivePrice,
        product.DiscountPercent,
        product.PrimaryImageUrl,
        product.Images.OrderBy(i => i.SortOrder).Select(i => i.Url).ToList(),
        product.AverageRating,
        product.RatingCount,
        product.SoldCount,
        product.StockQuantity,
        product.Category!.ToResponse(),
        product.Brand?.Name);

    public static CategoryResponse ToResponse(this Category category) =>
        new(category.Id, category.Name, category.Slug, category.IconUrl);
}
