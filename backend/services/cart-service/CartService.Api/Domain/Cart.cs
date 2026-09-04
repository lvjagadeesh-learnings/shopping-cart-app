namespace CartService.Api.Domain;

public sealed class Cart
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required Guid UserId { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public List<CartItem> Items { get; init; } = [];
}
