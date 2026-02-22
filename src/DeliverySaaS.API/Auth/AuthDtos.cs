namespace DeliverySaaS.API.Auth;

public sealed record LoginRequest(string UserName, string Password, Guid TenantId);
public sealed record RefreshRequest(string RefreshToken);
public sealed record LogoutRequest(string RefreshToken);

public sealed record AuthResponse(string AccessToken, string RefreshToken, DateTime ExpiresAtUtc);
