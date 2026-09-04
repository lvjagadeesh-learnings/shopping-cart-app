namespace AuthService.Api.Contracts;

public sealed record RegisterRequest(string Email, string Password, string FullName);

public sealed record LoginRequest(string Email, string Password);

public sealed record RefreshRequest(string RefreshToken);

public sealed record LogoutRequest(string RefreshToken);

public sealed record AuthResponse(string AccessToken, string RefreshToken, DateTimeOffset ExpiresAtUtc, UserResponse User);

public sealed record UserResponse(Guid Id, string Email, string FullName, string Role);
