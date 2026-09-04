using CartService.Api.Clients;
using CartService.Api.Contracts;
using CartService.Api.Data;
using CartService.Api.Domain;
using CartService.Api.Mapping;
using BuildingBlocks.Common;
using Microsoft.EntityFrameworkCore;

namespace CartService.Api.Services;

public sealed class CartManager(CartDbContext db, ICatalogServiceClient catalogClient)
{
    public async Task<CartResponse> GetCartAsync(Guid userId, CancellationToken ct)
    {
        var (cart, isNew) = await GetOrCreateCartAsync(userId, ct);
        if (isNew)
        {
            await db.SaveChangesAsync(ct);
        }

        return cart.ToResponse();
    }

    public async Task<CartResponse> AddItemAsync(Guid userId, AddCartItemRequest request, CancellationToken ct)
    {
        if (request.Quantity <= 0)
        {
            throw new ValidationApiException("Quantity must be greater than zero.");
        }

        var product = await catalogClient.GetProductAsync(request.ProductId, ct)
            ?? throw new NotFoundException($"Product '{request.ProductId}' was not found.");

        if (!product.InStock)
        {
            throw new ConflictException($"'{product.Name}' is currently out of stock.");
        }

        var (cart, _) = await GetOrCreateCartAsync(userId, ct);
        var existing = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId);

        if (existing is not null)
        {
            existing.Quantity += request.Quantity;
            existing.UnitPrice = product.EffectivePrice;
            existing.ProductName = product.Name;
            existing.ProductImageUrl = product.PrimaryImageUrl;
        }
        else
        {
            var newItem = new CartItem
            {
                CartId = cart.Id,
                ProductId = product.Id,
                ProductName = product.Name,
                ProductImageUrl = product.PrimaryImageUrl,
                UnitPrice = product.EffectivePrice,
                Quantity = request.Quantity
            };
            cart.Items.Add(newItem);
            db.CartItems.Add(newItem);
        }

        cart.UpdatedAtUtc = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);

        return cart.ToResponse();
    }

    public async Task<CartResponse> UpdateItemQuantityAsync(Guid userId, Guid productId, UpdateCartItemRequest request, CancellationToken ct)
    {
        if (request.Quantity < 0)
        {
            throw new ValidationApiException("Quantity cannot be negative.");
        }

        var (cart, _) = await GetOrCreateCartAsync(userId, ct);
        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId)
            ?? throw new NotFoundException("Cart item not found.");

        if (request.Quantity == 0)
        {
            cart.Items.Remove(item);
            db.CartItems.Remove(item);
        }
        else
        {
            item.Quantity = request.Quantity;
        }

        cart.UpdatedAtUtc = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);

        return cart.ToResponse();
    }

    public async Task<CartResponse> RemoveItemAsync(Guid userId, Guid productId, CancellationToken ct)
    {
        var (cart, _) = await GetOrCreateCartAsync(userId, ct);
        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId)
            ?? throw new NotFoundException("Cart item not found.");

        cart.Items.Remove(item);
        db.CartItems.Remove(item);
        cart.UpdatedAtUtc = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);

        return cart.ToResponse();
    }

    public async Task ClearCartAsync(Guid userId, CancellationToken ct)
    {
        var (cart, _) = await GetOrCreateCartAsync(userId, ct);
        db.CartItems.RemoveRange(cart.Items);
        cart.Items.Clear();
        cart.UpdatedAtUtc = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Returns the tracked cart for the user, creating (but not yet saving) one if none exists.
    /// A new cart is only added to the change tracker here; the caller performs the single
    /// SaveChangesAsync call for the whole operation so a fresh cart and its first item are
    /// persisted atomically. Checks already-tracked entries first so a cart created/loaded
    /// earlier in the same DbContext scope isn't re-fetched via a second Include query.
    /// </summary>
    private async Task<(Cart Cart, bool IsNew)> GetOrCreateCartAsync(Guid userId, CancellationToken ct)
    {
        var tracked = db.ChangeTracker.Entries<Cart>()
            .Select(e => e.Entity)
            .FirstOrDefault(c => c.UserId == userId);
        if (tracked is not null)
        {
            return (tracked, false);
        }

        var cart = await db.Carts.Include(c => c.Items).SingleOrDefaultAsync(c => c.UserId == userId, ct);
        if (cart is not null)
        {
            return (cart, false);
        }

        cart = new Cart { UserId = userId };
        db.Carts.Add(cart);
        return (cart, true);
    }
}
