namespace CatalogService.Api.Domain;

public sealed class ProductImage
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required Guid ProductId { get; init; }
    public required string Url { get; set; }
    public int SortOrder { get; set; }

    public Product? Product { get; init; }
}
