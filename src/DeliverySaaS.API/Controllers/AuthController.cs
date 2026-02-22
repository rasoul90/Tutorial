using System.Security.Cryptography;
using System.Text;
using DeliverySaaS.API.Auth;
using DeliverySaaS.API.Security;
using DeliverySaaS.Application.Common.Interfaces;
using DeliverySaaS.Domain.Identity.Entities;
using DeliverySaaS.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DeliverySaaS.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly RequestContext _requestContext;

    public AuthController(ApplicationDbContext dbContext, IPasswordHasher passwordHasher, IJwtTokenService jwtTokenService, RequestContext requestContext)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _requestContext = requestContext;
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == request.TenantId && x.UserName == request.UserName && x.IsActive, cancellationToken);

        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized();
        }

        var roleNames = await (from ur in _dbContext.UserRoles
                               join r in _dbContext.Roles on ur.RoleId equals r.Id
                               where ur.TenantId == request.TenantId && ur.UserId == user.Id
                               select r.Name)
            .Distinct()
            .ToListAsync(cancellationToken);

        var permissions = await (from ur in _dbContext.UserRoles
                                 join rp in _dbContext.RolePermissions on ur.RoleId equals rp.RoleId
                                 join p in _dbContext.Permissions on rp.PermissionId equals p.Id
                                 where ur.TenantId == request.TenantId && ur.UserId == user.Id
                                 select p.Key)
            .Distinct()
            .ToListAsync(cancellationToken);

        var (accessToken, expiresAtUtc) = _jwtTokenService.GenerateAccessToken(user, roleNames, permissions);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        _requestContext.TenantId = request.TenantId;
        _requestContext.BranchId = user.BranchId;

        _dbContext.RefreshTokens.Add(new RefreshToken
        {
            TenantId = request.TenantId,
            BranchId = user.BranchId,
            UserId = user.Id,
            TokenHash = ComputeHash(refreshToken),
            ExpiresAtUtc = DateTime.UtcNow.AddDays(14)
        });
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new AuthResponse(accessToken, refreshToken, expiresAtUtc));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh([FromBody] RefreshRequest request, CancellationToken cancellationToken)
    {
        var tokenHash = ComputeHash(request.RefreshToken);
        var refresh = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash && x.RevokedAtUtc == null, cancellationToken);

        if (refresh is null || refresh.ExpiresAtUtc <= DateTime.UtcNow)
        {
            return Unauthorized();
        }

        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == refresh.UserId && x.TenantId == refresh.TenantId, cancellationToken);
        if (user is null || !user.IsActive)
        {
            return Unauthorized();
        }

        _requestContext.TenantId = refresh.TenantId;
        _requestContext.BranchId = user.BranchId;

        refresh.RevokedAtUtc = DateTime.UtcNow;

        var roles = await (from ur in _dbContext.UserRoles
                           join r in _dbContext.Roles on ur.RoleId equals r.Id
                           where ur.TenantId == refresh.TenantId && ur.UserId == user.Id
                           select r.Name).Distinct().ToListAsync(cancellationToken);

        var permissions = await (from ur in _dbContext.UserRoles
                                 join rp in _dbContext.RolePermissions on ur.RoleId equals rp.RoleId
                                 join p in _dbContext.Permissions on rp.PermissionId equals p.Id
                                 where ur.TenantId == refresh.TenantId && ur.UserId == user.Id
                                 select p.Key).Distinct().ToListAsync(cancellationToken);

        var (accessToken, expiresAtUtc) = _jwtTokenService.GenerateAccessToken(user, roles, permissions);
        var newRefresh = _jwtTokenService.GenerateRefreshToken();
        _dbContext.RefreshTokens.Add(new RefreshToken
        {
            TenantId = refresh.TenantId,
            BranchId = user.BranchId,
            UserId = user.Id,
            TokenHash = ComputeHash(newRefresh),
            ExpiresAtUtc = DateTime.UtcNow.AddDays(14)
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Ok(new AuthResponse(accessToken, newRefresh, expiresAtUtc));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken cancellationToken)
    {
        var tokenHash = ComputeHash(request.RefreshToken);
        var refresh = await _dbContext.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == tokenHash && x.RevokedAtUtc == null, cancellationToken);
        if (refresh is null)
        {
            return NoContent();
        }

        _requestContext.TenantId = refresh.TenantId;
        refresh.RevokedAtUtc = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private static string ComputeHash(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes);
    }
}
