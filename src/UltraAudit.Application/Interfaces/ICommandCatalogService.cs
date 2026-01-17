using UltraAudit.Application.DTOs;

namespace UltraAudit.Application.Interfaces;

/// <summary>
/// خدمة كتالوج الأوامر للتحليلات.
/// </summary>
public interface ICommandCatalogService
{
    /// <summary>
    /// إرجاع جميع الأوامر المتاحة.
    /// </summary>
    Task<IReadOnlyList<CommandCatalogDto>> GetCatalogAsync(CancellationToken cancellationToken);
}
