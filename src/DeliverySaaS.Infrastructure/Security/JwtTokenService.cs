using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DeliverySaaS.Application.Common.Interfaces;
using DeliverySaaS.Domain.Identity.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DeliverySaaS.Infrastructure.Security;

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public (string AccessToken, DateTime ExpiresAtUtc) GenerateAccessToken(User user, IReadOnlyCollection<string> roles, IReadOnlyCollection<string> permissions)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("Jwt:SecretKey is missing.");
        var issuer = jwtSettings["Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer is missing.");
        var audience = jwtSettings["Audience"] ?? throw new InvalidOperationException("Jwt:Audience is missing.");
        var expiresMinutes = int.TryParse(jwtSettings["AccessTokenMinutes"], out var m) ? m : 30;

        var expiresAt = DateTime.UtcNow.AddMinutes(expiresMinutes);
        var claims = new List<Claim>
        {
            new("sub", user.Id.ToString()),
            new("tenant_id", user.TenantId.ToString()),
            new("username", user.UserName)
        };

        if (user.BranchId.HasValue)
        {
            claims.Add(new Claim("branch_id", user.BranchId.Value.ToString()));
        }

        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
        claims.AddRange(permissions.Select(p => new Claim("permission", p)));

        var creds = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(issuer, audience, claims, expires: expiresAt, signingCredentials: creds);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    public string GenerateRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
}
