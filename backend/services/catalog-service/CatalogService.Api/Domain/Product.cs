namespace CatalogService.Api.Domain;

public sealed class Product
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public string Description { get; set; } = string.Empty;
    public required decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public string PrimaryImageUrl { get; set; } = string.Empty;

    public required Guid CategoryId { get; set; }
    public Category? Category { get; init; }

    public Guid? BrandId { get; set; }
    public Brand? Brand { get; init; }

    /// <summary>Display-only cache; Inventory Service is the source of truth for real-time stock.</summary>
    public int StockQuantity { get; set; }
    public double AverageRating { get; set; }
    public int RatingCount { get; set; }
    public int SoldCount { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAtUtc { get; init; } = DateTimeOffset.UtcNow;

    public List<ProductImage> Images { get; init; } = [];

    public decimal EffectivePrice => DiscountPrice is > 0 && DiscountPrice < Price ? DiscountPrice.Value : Price;
    public int? DiscountPercent => DiscountPrice is > 0 && DiscountPrice < Price
        ? (int)Math.Round((1 - DiscountPrice.Value / Price) * 100)
        : null;
}
