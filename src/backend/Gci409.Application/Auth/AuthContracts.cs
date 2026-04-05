namespace Gci409.Application.Auth;

public sealed record RegisterUserRequest(string FullName, string Email, string Password);

public sealed record LoginRequest(string Email, string Password);

public sealed record RefreshAccessTokenRequest(string RefreshToken);

public sealed record AuthResponse(Guid UserId, string FullName, string Email, string AccessToken, string RefreshToken, DateTimeOffset ExpiresAtUtc);
