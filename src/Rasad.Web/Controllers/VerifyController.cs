using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.Tokens;
using Rasad.Domain.Entities;
using Rasad.Infrastructure.Identity;
using Rasad.Infrastructure.Security;
using Rasad.Web.Models.Api;
using Rasad.Web.Resources;

namespace Rasad.Web.Controllers;

[ApiController]
[Route("api/verify")]
[AllowAnonymous]
[EnableRateLimiting("verify")]
public class VerifyController : ControllerBase
{
    private static readonly TimeSpan AllowedSkew = TimeSpan.FromMinutes(5);
    private readonly ApplicationDbContext _context;
    private readonly IEncryptionService _encryptionService;
    private readonly IStringLocalizer<SharedResources> _localizer;
    private readonly IConfiguration _configuration;

    public VerifyController(ApplicationDbContext context, IEncryptionService encryptionService, IStringLocalizer<SharedResources> localizer, IConfiguration configuration)
    {
        _context = context;
        _encryptionService = encryptionService;
        _localizer = localizer;
        _configuration = configuration;
    }

    [HttpPost]
    public async Task<IActionResult> Verify([FromBody] VerifyRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new VerifyResponse { Message = _localizer["ApiValidationFailed"] });
        }

        var client = await _context.ApiClients.AsNoTracking().FirstOrDefaultAsync(x => x.ClientId == request.ClientId && x.IsActive);
        if (client is null)
        {
            return Unauthorized(new VerifyResponse { Message = _localizer["ApiUnauthorized"] });
        }

        if (client.MinistryId.HasValue && client.MinistryId.Value != request.MinistryId)
        {
            await LogAuditAsync(request.ClientId, "ApiVerify", "ApiClient", client.Id.ToString(), _localizer["ApiClientMinistryMismatch"]);
            return Forbid();
        }

        if (!ValidateJwt(request.Token))
        {
            return Unauthorized(new VerifyResponse { Message = _localizer["ApiInvalidToken"] });
        }

        if (!TryParseTimestamp(request.Timestamp, out var timestamp))
        {
            return BadRequest(new VerifyResponse { Message = _localizer["ApiInvalidTimestamp"] });
        }

        var now = DateTimeOffset.UtcNow;
        if (now - timestamp > AllowedSkew || timestamp - now > AllowedSkew)
        {
            return BadRequest(new VerifyResponse { Message = _localizer["ApiExpiredTimestamp"] });
        }

        var nonceExists = await _context.ApiNonces.AnyAsync(x => x.ApiClientId == client.Id && x.Nonce == request.Nonce);
        if (nonceExists)
        {
            return BadRequest(new VerifyResponse { Message = _localizer["ApiReplayDetected"] });
        }

        var payload = BuildPayload(request);
        var signature = HmacSignatureService.ComputeSignature(Convert.ToBase64String(client.SecretHash), payload);
        if (!CryptographicEquals(signature, request.Signature))
        {
            return Unauthorized(new VerifyResponse { Message = _localizer["ApiInvalidSignature"] });
        }

        var nonce = new ApiNonce
        {
            Id = Guid.NewGuid(),
            ApiClientId = client.Id,
            Nonce = request.Nonce,
            Timestamp = timestamp.UtcDateTime,
            ExpiresAt = now.UtcDateTime.Add(AllowedSkew),
            IsUsed = true
        };

        _context.ApiNonces.Add(nonce);

        var nationalIdHash = _encryptionService.ComputeHash(request.NationalId);
        var employee = await _context.Employees
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.MinistryId == request.MinistryId && x.NationalIdHash == nationalIdHash);

        if (employee is null)
        {
            await _context.SaveChangesAsync();
            return Ok(new VerifyResponse { Message = _localizer["ApiNoMatch"] });
        }

        if (employee.IsVerified)
        {
            await _context.SaveChangesAsync();
            return Ok(new VerifyResponse { Message = _localizer["ApiAlreadyVerified"] });
        }

        employee.IsVerified = true;
        employee.VerifiedAt = DateTime.UtcNow;
        employee.VerifiedSource = "App";
        employee.VerifiedByUserId = request.ClientId;

        await LogAuditAsync(request.ClientId, "Verify", nameof(Employee), employee.Id.ToString(), _localizer["ApiVerifiedSuccess"]);
        await _context.SaveChangesAsync();

        return Ok(new VerifyResponse { Message = _localizer["ApiVerifiedSuccess"] });
    }

    private bool ValidateJwt(string token)
    {
        var key = _configuration["Jwt:Key"] ?? string.Empty;
        if (string.IsNullOrWhiteSpace(key))
        {
            return false;
        }

        var handler = new JwtSecurityTokenHandler();
        try
        {
            handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(key)),
                ClockSkew = TimeSpan.FromMinutes(1)
            }, out _);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool TryParseTimestamp(string value, out DateTimeOffset timestamp)
    {
        if (DateTimeOffset.TryParse(value, out timestamp))
        {
            return true;
        }

        return DateTimeOffset.TryParseExact(value, "yyyyMMddHHmmss", null, System.Globalization.DateTimeStyles.AssumeUniversal, out timestamp);
    }

    private static string BuildPayload(VerifyRequest request)
    {
        return $"{request.ClientId}|{request.MinistryId}|{request.NationalId}|{request.Timestamp}|{request.Nonce}";
    }

    private static bool CryptographicEquals(string left, string right)
    {
        var leftBytes = System.Text.Encoding.UTF8.GetBytes(left);
        var rightBytes = System.Text.Encoding.UTF8.GetBytes(right);
        return CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
    }

    private async Task LogAuditAsync(string userId, string action, string entityName, string entityId, string details)
    {
        var audit = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ActionType = action,
            EntityName = entityName,
            EntityId = entityId,
            Timestamp = DateTime.UtcNow,
            Details = details,
            IP = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty
        };

        _context.AuditLogs.Add(audit);
        await _context.SaveChangesAsync();
    }
}
