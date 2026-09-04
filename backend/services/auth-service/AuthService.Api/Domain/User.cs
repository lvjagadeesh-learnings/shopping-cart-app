namespace AuthService.Api.Domain;

public sealed class User
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public required string FullName { get; set; }
    public string Role { get; set; } = BuildingBlocks.Auth.Roles.Customer;
    public DateTimeOffset CreatedAtUtc { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? LastLoginAtUtc { get; set; }

    public List<RefreshToken> RefreshTokens { get; init; } = [];
}
