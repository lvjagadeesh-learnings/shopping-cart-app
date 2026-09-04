namespace CatalogService.Api.Domain;

public sealed class Category
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public string? IconUrl { get; set; }
    public int SortOrder { get; set; }

    public List<Product> Products { get; init; } = [];
}
