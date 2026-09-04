using BuildingBlocks.Auth;
using BuildingBlocks.Common;
using CatalogService.Api.Contracts;
using CatalogService.Api.Data;
using CatalogService.Api.Domain;
using CatalogService.Api.Mapping;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Api.Endpoints;

public static class CatalogEndpoints
{
    public static void MapCatalogEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/catalog").WithTags("Catalog");

        group.MapGet("/categories", async (CatalogDbContext db, CancellationToken ct) =>
            Results.Ok(await db.Categories
                .OrderBy(c => c.SortOrder)
                .Select(c => c.ToResponse())
                .ToListAsync(ct)));

        group.MapGet("/products", async (
            CatalogDbContext db,
            CancellationToken ct,
            string? q,
            string? category,
            string? sort,
            int page = 1,
            int pageSize = 20) =>
        {
            var paging = new PagingQuery { Page = page, PageSize = pageSize };

            var query = db.Products.Where(p => p.IsActive).AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = $"%{q.Trim()}%";
                query = query.Where(p => EF.Functions.ILike(p.Name, term));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(p => p.Category!.Slug == category);
            }

            query = sort switch
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "rating" => query.OrderByDescending(p => p.AverageRating),
                "newest" => query.OrderByDescending(p => p.CreatedAtUtc),
                _ => query.OrderByDescending(p => p.SoldCount)
            };

            var total = await query.CountAsync(ct);
            var items = await query.Skip(paging.Skip).Take(paging.PageSize)
                .Select(p => p.ToSummary())
                .ToListAsync(ct);

            return Results.Ok(new PagedResult<ProductSummaryResponse>(items, paging.Page, paging.PageSize, total));
        });

        group.MapGet("/products/{slug}", async (string slug, CatalogDbContext db, CancellationToken ct) =>
        {
            var product = await db.Products
                .Include(p => p.Images)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .SingleOrDefaultAsync(p => p.Slug == slug && p.IsActive, ct)
                ?? throw new NotFoundException($"Product '{slug}' was not found.");

            return Results.Ok(product.ToDetail());
        });

        // Internal lookup by id — used by other services (e.g. Cart) that only hold a ProductId.
        group.MapGet("/products/id/{id:guid}", async (Guid id, CatalogDbContext db, CancellationToken ct) =>
        {
            var product = await db.Products
                .SingleOrDefaultAsync(p => p.Id == id && p.IsActive, ct)
                ?? throw new NotFoundException($"Product '{id}' was not found.");

            return Results.Ok(product.ToSummary());
        });

        // Internal lookup — used by Recommendation Service to find a product's category for "related products".
        group.MapGet("/products/id/{id:guid}/category", async (Guid id, CatalogDbContext db, CancellationToken ct) =>
        {
            var product = await db.Products
                .Include(p => p.Category)
                .SingleOrDefaultAsync(p => p.Id == id && p.IsActive, ct)
                ?? throw new NotFoundException($"Product '{id}' was not found.");

            return Results.Ok(product.Category!.ToResponse());
        });

        group.MapPost("/products", async (UpsertProductRequest request, CatalogDbContext db, CancellationToken ct) =>
        {
            _ = await db.Categories.FindAsync([request.CategoryId], ct)
                ?? throw new NotFoundException("Category not found.");

            var product = new Product
            {
                Name = request.Name,
                Slug = Slugify(request.Name),
                Description = request.Description,
                Price = request.Price,
                DiscountPrice = request.DiscountPrice,
                PrimaryImageUrl = request.PrimaryImageUrl,
                CategoryId = request.CategoryId,
                BrandId = request.BrandId,
                StockQuantity = request.StockQuantity
            };

            db.Products.Add(product);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/catalog/products/{product.Slug}", product.ToSummary());
        }).RequireAuthorization(Policies.Admin);

        group.MapPut("/products/{id:guid}", async (Guid id, UpsertProductRequest request, CatalogDbContext db, CancellationToken ct) =>
        {
            var product = await db.Products.FindAsync([id], ct)
                ?? throw new NotFoundException("Product not found.");

            product.Name = request.Name;
            product.Description = request.Description;
            product.Price = request.Price;
            product.DiscountPrice = request.DiscountPrice;
            product.PrimaryImageUrl = request.PrimaryImageUrl;
            product.CategoryId = request.CategoryId;
            product.BrandId = request.BrandId;
            product.StockQuantity = request.StockQuantity;

            await db.SaveChangesAsync(ct);
            return Results.Ok(product.ToSummary());
        }).RequireAuthorization(Policies.Admin);

        group.MapDelete("/products/{id:guid}", async (Guid id, CatalogDbContext db, CancellationToken ct) =>
        {
            var product = await db.Products.FindAsync([id], ct)
                ?? throw new NotFoundException("Product not found.");

            product.IsActive = false;
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        }).RequireAuthorization(Policies.Admin);
    }

    private static string Slugify(string name) =>
        string.Concat(name.ToLowerInvariant().Select(c => char.IsLetterOrDigit(c) ? c : '-'))
            .Replace("--", "-").Trim('-') + "-" + Guid.NewGuid().ToString("N")[..6];
}
