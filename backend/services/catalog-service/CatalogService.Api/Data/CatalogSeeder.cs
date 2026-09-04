using CatalogService.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Api.Data;

/// <summary>
/// Dev/demo seed data — realistic-looking marketplace catalog (generic product names, no
/// trademarked brand/content) with Picsum placeholder images so the storefront renders like a
/// real e-commerce catalog out of the box.
/// </summary>
public static class CatalogSeeder
{
    public static async Task SeedAsync(CatalogDbContext db, CancellationToken ct = default)
    {
        if (await db.Categories.AnyAsync(ct)) return;

        var categories = new[]
        {
            new Category { Name = "Mobile & Gadgets", Slug = "mobile-gadgets", IconUrl = "https://picsum.photos/seed/cat-mobile/64", SortOrder = 1 },
            new Category { Name = "Electronics", Slug = "electronics", IconUrl = "https://picsum.photos/seed/cat-electronics/64", SortOrder = 2 },
            new Category { Name = "Women's Fashion", Slug = "womens-fashion", IconUrl = "https://picsum.photos/seed/cat-womens/64", SortOrder = 3 },
            new Category { Name = "Men's Fashion", Slug = "mens-fashion", IconUrl = "https://picsum.photos/seed/cat-mens/64", SortOrder = 4 },
            new Category { Name = "Home & Living", Slug = "home-living", IconUrl = "https://picsum.photos/seed/cat-home/64", SortOrder = 5 },
            new Category { Name = "Health & Beauty", Slug = "health-beauty", IconUrl = "https://picsum.photos/seed/cat-beauty/64", SortOrder = 6 },
            new Category { Name = "Groceries & Pets", Slug = "groceries-pets", IconUrl = "https://picsum.photos/seed/cat-grocery/64", SortOrder = 7 },
            new Category { Name = "Toys, Kids & Babies", Slug = "toys-kids-babies", IconUrl = "https://picsum.photos/seed/cat-toys/64", SortOrder = 8 },
            new Category { Name = "Sports & Outdoor", Slug = "sports-outdoor", IconUrl = "https://picsum.photos/seed/cat-sports/64", SortOrder = 9 },
        };
        db.Categories.AddRange(categories);

        var brands = new[] { "Nova", "Zenlite", "UrbanCraft", "Everfit", "Homely", "PurePetal", "GreenLeaf", "PlayNest", "TrailBlazer" }
            .Select(name => new Brand { Name = name }).ToArray();
        db.Brands.AddRange(brands);

        Category Cat(string slug) => categories.First(c => c.Slug == slug);
        Brand? Brand(string name) => brands.FirstOrDefault(b => b.Name == name);

        var rng = new Random(42);
        var products = new List<Product>
        {
            Make("Wireless Noise-Cancelling Headphones", "mobile-gadgets", "Nova", 89.99m, 59.99m, 4.7, 1240, 3500),
            Make("6.7\" Smartphone 128GB", "mobile-gadgets", "Nova", 399.00m, 329.00m, 4.5, 860, 1200),
            Make("USB-C Fast Charger 65W", "mobile-gadgets", "Zenlite", 24.99m, null, 4.6, 2100, 5400),
            Make("Smartwatch Fitness Tracker", "mobile-gadgets", "Nova", 59.99m, 44.99m, 4.4, 980, 2600),
            Make("Bluetooth Portable Speaker", "electronics", "Zenlite", 34.99m, 27.99m, 4.3, 640, 1500),
            Make("27\" 4K Monitor", "electronics", "Zenlite", 249.00m, 219.00m, 4.6, 310, 480),
            Make("Mechanical Gaming Keyboard", "electronics", "UrbanCraft", 54.99m, 39.99m, 4.5, 720, 1900),
            Make("Wireless Mouse Ergonomic", "electronics", "UrbanCraft", 19.99m, null, 4.4, 1500, 4200),
            Make("Women's Oversized Denim Jacket", "womens-fashion", "UrbanCraft", 45.00m, 32.00m, 4.3, 210, 890),
            Make("Women's Summer Floral Dress", "womens-fashion", "UrbanCraft", 29.99m, 21.99m, 4.5, 430, 1600),
            Make("Women's Canvas Tote Bag", "womens-fashion", "Everfit", 18.50m, null, 4.2, 320, 1100),
            Make("Women's Running Sneakers", "womens-fashion", "Everfit", 55.00m, 39.99m, 4.6, 540, 1300),
            Make("Men's Slim Fit Chino Pants", "mens-fashion", "UrbanCraft", 32.00m, 24.99m, 4.3, 260, 780),
            Make("Men's Cotton Polo Shirt", "mens-fashion", "Everfit", 22.00m, 16.99m, 4.4, 410, 1450),
            Make("Men's Leather Wallet", "mens-fashion", "UrbanCraft", 27.99m, null, 4.5, 190, 620),
            Make("Men's Sports Sneakers", "mens-fashion", "Everfit", 49.99m, 34.99m, 4.6, 610, 2000),
            Make("Non-Stick Cookware Set 10pc", "home-living", "Homely", 79.99m, 59.99m, 4.7, 340, 700),
            Make("Memory Foam Pillow", "home-living", "Homely", 22.99m, 17.99m, 4.5, 890, 2300),
            Make("LED Desk Lamp Dimmable", "home-living", "Homely", 24.99m, null, 4.4, 260, 640),
            Make("Storage Organizer Box Set", "home-living", "Homely", 15.99m, 11.99m, 4.3, 470, 1100),
            Make("Vitamin C Brightening Serum", "health-beauty", "PurePetal", 18.99m, 13.99m, 4.6, 1350, 4100),
            Make("Hydrating Facial Sheet Mask (10pk)", "health-beauty", "PurePetal", 9.99m, 6.99m, 4.5, 980, 3200),
            Make("Electric Toothbrush Rechargeable", "health-beauty", "PurePetal", 29.99m, 22.99m, 4.6, 520, 1400),
            Make("Hair Dryer Ionic 1800W", "health-beauty", "Nova", 34.99m, 26.99m, 4.4, 380, 900),
            Make("Premium Roasted Coffee Beans 1kg", "groceries-pets", "GreenLeaf", 16.99m, null, 4.7, 610, 2200),
            Make("Grain-Free Dog Food 5kg", "groceries-pets", "GreenLeaf", 32.99m, 27.99m, 4.6, 290, 610),
            Make("Cat Scratching Post Tower", "groceries-pets", "GreenLeaf", 39.99m, 29.99m, 4.5, 210, 480),
            Make("Organic Green Tea (100 bags)", "groceries-pets", "GreenLeaf", 12.99m, 9.99m, 4.4, 450, 1350),
            Make("Wooden Building Blocks Set", "toys-kids-babies", "PlayNest", 21.99m, 16.99m, 4.7, 340, 980),
            Make("Baby Diaper Bag Backpack", "toys-kids-babies", "PlayNest", 36.99m, 28.99m, 4.6, 260, 640),
            Make("Remote Control Racing Car", "toys-kids-babies", "PlayNest", 27.99m, 19.99m, 4.4, 410, 1250),
            Make("Yoga Mat Non-Slip 6mm", "sports-outdoor", "TrailBlazer", 19.99m, 14.99m, 4.6, 720, 2100),
            Make("Insulated Water Bottle 1L", "sports-outdoor", "TrailBlazer", 14.99m, 10.99m, 4.5, 980, 3600),
            Make("Adjustable Dumbbell Set 20kg", "sports-outdoor", "TrailBlazer", 89.99m, 69.99m, 4.7, 180, 320),
            Make("Camping Tent 4-Person", "sports-outdoor", "TrailBlazer", 119.00m, 94.99m, 4.6, 130, 210),
        };

        db.Products.AddRange(products);
        await db.SaveChangesAsync(ct);
        return;

        Product Make(string name, string categorySlug, string brandName, decimal price, decimal? discountPrice, double rating, int ratingCount, int soldCount)
        {
            var seed = Guid.NewGuid().ToString("N")[..8];
            var product = new Product
            {
                Name = name,
                Slug = Slugify(name, seed),
                Description = $"{name} — quality guaranteed, fast shipping, and hassle-free returns.",
                Price = price,
                DiscountPrice = discountPrice,
                PrimaryImageUrl = $"https://picsum.photos/seed/{seed}/480/480",
                CategoryId = Cat(categorySlug).Id,
                BrandId = Brand(brandName)?.Id,
                StockQuantity = rng.Next(15, 500),
                AverageRating = rating,
                RatingCount = ratingCount,
                SoldCount = soldCount,
            };

            product.Images.Add(new ProductImage { ProductId = product.Id, Url = $"https://picsum.photos/seed/{seed}-2/480/480", SortOrder = 1 });
            product.Images.Add(new ProductImage { ProductId = product.Id, Url = $"https://picsum.photos/seed/{seed}-3/480/480", SortOrder = 2 });

            return product;
        }
    }

    private static string Slugify(string name, string seed) =>
        string.Concat(name.ToLowerInvariant().Select(c => char.IsLetterOrDigit(c) ? c : '-'))
            .Replace("--", "-").Trim('-') + "-" + seed;
}
