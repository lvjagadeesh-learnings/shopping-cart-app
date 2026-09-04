using AuthService.Api.Contracts;
using AuthService.Api.Data;
using AuthService.Api.Domain;
using AuthService.Api.Services;
using BuildingBlocks.Auth;
using BuildingBlocks.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Tests;

public class AuthTokenServiceTests
{
    private static AuthTokenService CreateService(out AuthDbContext db)
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        db = new AuthDbContext(options);

        var jwtOptions = new JwtOptions
        {
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            SigningKey = "unit-test-signing-key-not-for-production-use-32chars+",
            AccessTokenMinutes = 15,
            RefreshTokenDays = 7
        };

        var tokenGenerator = new JwtTokenGenerator(jwtOptions);
        var passwordHasher = new PasswordHasher<User>();

        return new AuthTokenService(db, jwtOptions, tokenGenerator, passwordHasher);
    }

    [Fact]
    public async Task RegisterAsync_NewEmail_CreatesUserAndReturnsTokens()
    {
        var sut = CreateService(out var db);
        var request = new RegisterRequest("new.user@example.com", "P@ssw0rd123", "New User");

        var result = await sut.RegisterAsync(request, CancellationToken.None);

        Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(result.RefreshToken));
        Assert.Equal("new.user@example.com", result.User.Email);
        Assert.Single(await db.Users.ToListAsync());
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ThrowsConflictException()
    {
        var sut = CreateService(out _);
        var request = new RegisterRequest("dup@example.com", "P@ssw0rd123", "Dup User");
        await sut.RegisterAsync(request, CancellationToken.None);

        await Assert.ThrowsAsync<ConflictException>(
            () => sut.RegisterAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task RegisterAsync_EmailIsNormalizedToLowercase()
    {
        var sut = CreateService(out var db);
        var request = new RegisterRequest("Mixed.Case@Example.com", "P@ssw0rd123", "Case User");

        await sut.RegisterAsync(request, CancellationToken.None);

        var user = await db.Users.SingleAsync();
        Assert.Equal("mixed.case@example.com", user.Email);
    }

    [Fact]
    public async Task LoginAsync_CorrectCredentials_ReturnsTokensAndUpdatesLastLogin()
    {
        var sut = CreateService(out var db);
        await sut.RegisterAsync(new RegisterRequest("login@example.com", "P@ssw0rd123", "Login User"), CancellationToken.None);

        var result = await sut.LoginAsync(new LoginRequest("login@example.com", "P@ssw0rd123"), CancellationToken.None);

        Assert.Equal("login@example.com", result.User.Email);
        var user = await db.Users.SingleAsync();
        Assert.NotNull(user.LastLoginAtUtc);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsUnauthorizedApiException()
    {
        var sut = CreateService(out _);
        await sut.RegisterAsync(new RegisterRequest("wrongpw@example.com", "P@ssw0rd123", "User"), CancellationToken.None);

        await Assert.ThrowsAsync<UnauthorizedApiException>(
            () => sut.LoginAsync(new LoginRequest("wrongpw@example.com", "IncorrectPassword"), CancellationToken.None));
    }

    [Fact]
    public async Task LoginAsync_UnknownEmail_ThrowsUnauthorizedApiException()
    {
        var sut = CreateService(out _);

        await Assert.ThrowsAsync<UnauthorizedApiException>(
            () => sut.LoginAsync(new LoginRequest("nobody@example.com", "P@ssw0rd123"), CancellationToken.None));
    }

    [Fact]
    public async Task RefreshAsync_ValidToken_RotatesTokenAndReturnsNewPair()
    {
        var sut = CreateService(out var db);
        var registered = await sut.RegisterAsync(new RegisterRequest("refresh@example.com", "P@ssw0rd123", "Refresh User"), CancellationToken.None);

        var refreshed = await sut.RefreshAsync(new RefreshRequest(registered.RefreshToken), CancellationToken.None);

        Assert.NotEqual(registered.RefreshToken, refreshed.RefreshToken);
        Assert.NotEqual(registered.AccessToken, refreshed.AccessToken);

        // Original refresh token must now be revoked and unusable.
        await Assert.ThrowsAsync<UnauthorizedApiException>(
            () => sut.RefreshAsync(new RefreshRequest(registered.RefreshToken), CancellationToken.None));

        Assert.Equal(2, await db.RefreshTokens.CountAsync());
    }

    [Fact]
    public async Task RefreshAsync_UnknownToken_ThrowsUnauthorizedApiException()
    {
        var sut = CreateService(out _);

        await Assert.ThrowsAsync<UnauthorizedApiException>(
            () => sut.RefreshAsync(new RefreshRequest("not-a-real-refresh-token"), CancellationToken.None));
    }

    [Fact]
    public async Task LogoutAsync_RevokesMatchingToken()
    {
        var sut = CreateService(out var db);
        var registered = await sut.RegisterAsync(new RegisterRequest("logout@example.com", "P@ssw0rd123", "Logout User"), CancellationToken.None);

        await sut.LogoutAsync(new LogoutRequest(registered.RefreshToken), CancellationToken.None);

        var token = await db.RefreshTokens.SingleAsync();
        Assert.NotNull(token.RevokedAtUtc);
        Assert.False(token.IsActive);
    }

    [Fact]
    public async Task LogoutAsync_UnknownToken_DoesNotThrow()
    {
        var sut = CreateService(out _);

        var exception = await Record.ExceptionAsync(
            () => sut.LogoutAsync(new LogoutRequest("never-issued-token"), CancellationToken.None));

        Assert.Null(exception);
    }
}
