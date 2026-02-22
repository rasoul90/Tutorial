using DeliverySaaS.Domain.Identity.Entities;

namespace DeliverySaaS.Application.Common.Interfaces;

public interface IJwtTokenService
{
    (string AccessToken, DateTime ExpiresAtUtc) GenerateAccessToken(User user, IReadOnlyCollection<string> roles, IReadOnlyCollection<string> permissions);
    string GenerateRefreshToken();
}
