namespace DeliverySaaS.Web.Models;

public sealed record LoginRequest(string UserName, string Password, Guid TenantId);
public sealed record AuthResponse(string AccessToken, string RefreshToken, DateTime ExpiresAtUtc);
