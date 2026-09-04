using System.Security.Cryptography;
using System.Text;
using AuthService.Api.Contracts;
using AuthService.Api.Data;
using AuthService.Api.Domain;
using BuildingBlocks.Auth;
using BuildingBlocks.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Api.Services;

public sealed class AuthTokenService(
    AuthDbContext db,
    JwtOptions jwtOptions,
    JwtTokenGenerator tokenGenerator,
    PasswordHasher<User> passwordHasher)
{
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (await db.Users.AnyAsync(u => u.Email == email, ct))
        {
            throw new ConflictException("An account with this email already exists.");
        }

        var user = new User
        {
            Email = email,
            FullName = request.FullName.Trim(),
            PasswordHash = string.Empty
        };
        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        return await IssueTokensAsync(user, ct);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.SingleOrDefaultAsync(u => u.Email == email, ct)
            ?? throw new UnauthorizedApiException("Invalid email or password.");

        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedApiException("Invalid email or password.");
        }

        user.LastLoginAtUtc = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);

        return await IssueTokensAsync(user, ct);
    }

    public async Task<AuthResponse> RefreshAsync(RefreshRequest request, CancellationToken ct)
    {
        var tokenHash = Hash(request.RefreshToken);
        var existing = await db.RefreshTokens
            .Include(rt => rt.User)
            .SingleOrDefaultAsync(rt => rt.TokenHash == tokenHash, ct)
            ?? throw new UnauthorizedApiException("Invalid refresh token.");

        if (!existing.IsActive || existing.User is null)
        {
            throw new UnauthorizedApiException("Refresh token expired or revoked.");
        }

        existing.RevokedAtUtc = DateTimeOffset.UtcNow;
        return await IssueTokensAsync(existing.User, ct);
    }

    public async Task LogoutAsync(LogoutRequest request, CancellationToken ct)
    {
        var tokenHash = Hash(request.RefreshToken);
        var existing = await db.RefreshTokens.SingleOrDefaultAsync(rt => rt.TokenHash == tokenHash, ct);
        if (existing is not null)
        {
            existing.RevokedAtUtc = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(ct);
        }
    }

    private async Task<AuthResponse> IssueTokensAsync(User user, CancellationToken ct)
    {
        var accessToken = tokenGenerator.CreateAccessToken(user.Id, user.Email, user.Role);
        var rawRefreshToken = JwtTokenGenerator.CreateRefreshToken();

        db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = Hash(rawRefreshToken),
            ExpiresAtUtc = DateTimeOffset.UtcNow.AddDays(jwtOptions.RefreshTokenDays)
        });
        await db.SaveChangesAsync(ct);

        return new AuthResponse(
            accessToken,
            rawRefreshToken,
            DateTimeOffset.UtcNow.AddMinutes(jwtOptions.AccessTokenMinutes),
            new UserResponse(user.Id, user.Email, user.FullName, user.Role));
    }

    private static string Hash(string value) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}
