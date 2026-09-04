using CatalogService.Api.Data;
using CatalogService.Api.Domain;
using CatalogService.Api.Mapping;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Tests;

public class ProductTests
{
    [Fact]
    public void EffectivePrice_WithLowerDiscount_ReturnsDiscountPrice()
    {
        var product = new Product { Name = "Test", Slug = "test", Price = 100m, DiscountPrice = 75m, CategoryId = Guid.NewGuid() };

        Assert.Equal(75m, product.EffectivePrice);
    }

    [Fact]
    public void EffectivePrice_WithNoDiscount_ReturnsPrice()
    {
        var product = new Product { Name = "Test", Slug = "test", Price = 100m, CategoryId = Guid.NewGuid() };

        Assert.Equal(100m, product.EffectivePrice);
    }

    [Fact]
    public void EffectivePrice_WithDiscountHigherThanPrice_ReturnsPrice()
    {
        var product = new Product { Name = "Test", Slug = "test", Price = 100m, DiscountPrice = 150m, CategoryId = Guid.NewGuid() };

        Assert.Equal(100m, product.EffectivePrice);
    }

    [Fact]
    public void DiscountPercent_WithValidDiscount_CalculatesRoundedPercentage()
    {
        var product = new Product { Name = "Test", Slug = "test", Price = 100m, DiscountPrice = 75m, CategoryId = Guid.NewGuid() };

        Assert.Equal(25, product.DiscountPercent);
    }

    [Fact]
    public void DiscountPercent_WithNoDiscount_ReturnsNull()
    {
        var product = new Product { Name = "Test", Slug = "test", Price = 100m, CategoryId = Guid.NewGuid() };

        Assert.Null(product.DiscountPercent);
    }
}

public class ProductMappingTests
{
    [Fact]
    public void ToSummary_MapsFieldsAndInStockFlag()
    {
        var product = new Product
        {
            Name = "Widget",
            Slug = "widget-abc123",
            Price = 50m,
            DiscountPrice = 40m,
            PrimaryImageUrl = "https://picsum.photos/seed/x/480/480",
            CategoryId = Guid.NewGuid(),
            StockQuantity = 10,
            AverageRating = 4.5,
            RatingCount = 20,
            SoldCount = 100
        };

        var summary = product.ToSummary();

        Assert.Equal(product.Id, summary.Id);
        Assert.Equal("Widget", summary.Name);
        Assert.Equal(40m, summary.EffectivePrice);
        Assert.Equal(20, summary.DiscountPercent);
        Assert.True(summary.InStock);
    }

    [Fact]
    public void ToSummary_ZeroStock_InStockIsFalse()
    {
        var product = new Product { Name = "Widget", Slug = "widget", Price = 10m, CategoryId = Guid.NewGuid(), StockQuantity = 0 };

        Assert.False(product.ToSummary().InStock);
    }

    [Fact]
    public void ToDetail_IncludesOrderedImagesAndCategory()
    {
        var category = new Category { Name = "Gadgets", Slug = "gadgets" };
        var product = new Product
        {
            Name = "Widget",
            Slug = "widget",
            Price = 10m,
            CategoryId = category.Id,
            Category = category
        };
        product.Images.Add(new ProductImage { ProductId = product.Id, Url = "https://img/2", SortOrder = 2 });
        product.Images.Add(new ProductImage { ProductId = product.Id, Url = "https://img/1", SortOrder = 1 });

        var detail = product.ToDetail();

        Assert.Equal(["https://img/1", "https://img/2"], detail.Images);
        Assert.Equal("Gadgets", detail.Category.Name);
    }

    [Fact]
    public void ToResponse_MapsCategoryFields()
    {
        var category = new Category { Name = "Electronics", Slug = "electronics", IconUrl = "icon.png", SortOrder = 3 };

        var response = category.ToResponse();

        Assert.Equal(category.Id, response.Id);
        Assert.Equal("Electronics", response.Name);
        Assert.Equal("electronics", response.Slug);
        Assert.Equal("icon.png", response.IconUrl);
    }
}

public class CatalogSeederTests
{
    private static CatalogDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new CatalogDbContext(options);
    }

    [Fact]
    public async Task SeedAsync_PopulatesCategoriesBrandsAndProducts()
    {
        using var db = CreateDb();

        await CatalogSeeder.SeedAsync(db);

        Assert.Equal(9, await db.Categories.CountAsync());
        Assert.Equal(9, await db.Brands.CountAsync());
        Assert.True(await db.Products.CountAsync() > 0);
    }

    [Fact]
    public async Task SeedAsync_EachProductHasValidCategoryAndTwoExtraImages()
    {
        using var db = CreateDb();
        await CatalogSeeder.SeedAsync(db);

        var categoryIds = await db.Categories.Select(c => c.Id).ToListAsync();
        var products = await db.Products.Include(p => p.Images).ToListAsync();

        Assert.All(products, p =>
        {
            Assert.Contains(p.CategoryId, categoryIds);
            Assert.Equal(2, p.Images.Count);
            Assert.All(p.Images, img => Assert.Equal(p.Id, img.ProductId));
        });
    }

    [Fact]
    public async Task SeedAsync_IsIdempotent_DoesNotDuplicateWhenRunTwice()
    {
        using var db = CreateDb();
        await CatalogSeeder.SeedAsync(db);
        var firstCount = await db.Products.CountAsync();

        await CatalogSeeder.SeedAsync(db);
        var secondCount = await db.Products.CountAsync();

        Assert.Equal(firstCount, secondCount);
    }
}
