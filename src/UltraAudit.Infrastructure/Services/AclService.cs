using Microsoft.EntityFrameworkCore;
using UltraAudit.Application.Interfaces;
using UltraAudit.Infrastructure.Data;

namespace UltraAudit.Infrastructure.Services;

/// <summary>
/// تنفيذ خدمة التحكم في الوصول المبني على الموارد.
/// </summary>
public sealed class AclService : IAclService
{
    private readonly UasDbContext _dbContext;

    public AclService(UasDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<bool> HasPermissionAsync(string userId, string resourceType, string resourceId, string permission, CancellationToken cancellationToken)
    {
        // تحقق ACL على مستوى المورد مع قابلية دمج أدوار إضافية.
        return await _dbContext.ResourceGrants.AnyAsync(
            grant => grant.PrincipalId == userId
                     && grant.ResourceType == resourceType
                     && grant.ResourceId == resourceId
                     && grant.Permission == permission,
            cancellationToken);
    }
}
