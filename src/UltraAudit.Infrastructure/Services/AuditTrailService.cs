using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using UltraAudit.Application.Interfaces;
using UltraAudit.Domain.Entities;
using UltraAudit.Infrastructure.Data;

namespace UltraAudit.Infrastructure.Services;

/// <summary>
/// تنفيذ خدمة سجل التدقيق مع بصمة تكامل.
/// </summary>
public sealed class AuditTrailService : IAuditTrailService
{
    private readonly UasDbContext _dbContext;

    public AuditTrailService(UasDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task RecordAsync(string action, string entityName, string entityId, string userId, string metadataJson, CancellationToken cancellationToken)
    {
        var entry = new AuditTrailEntry
        {
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            PerformedByUserId = userId,
            MetadataJson = metadataJson,
        };

        // حساب بصمة التكامل لضمان عدم العبث بسجل التدقيق.
        entry.IntegrityHash = ComputeIntegrityHash(entry);
        _dbContext.AuditTrail.Add(entry);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string ComputeIntegrityHash(AuditTrailEntry entry)
    {
        var payload = $"{entry.Action}|{entry.EntityName}|{entry.EntityId}|{entry.PerformedByUserId}|{entry.MetadataJson}|{entry.CreatedAtUtc:o}";
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload)));
    }
}
