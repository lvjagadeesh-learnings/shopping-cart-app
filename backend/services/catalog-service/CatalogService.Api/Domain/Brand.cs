namespace CatalogService.Api.Domain;

public sealed class Brand
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Name { get; set; }

    public List<Product> Products { get; init; } = [];
}
