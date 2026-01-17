namespace UltraAudit.Domain.Entities;

/// <summary>
/// كيان أساسي يحتوي على الخصائص المشتركة لكل الكيانات.
/// </summary>
public abstract class EntityBase
{
    /// <summary>
    /// المعرّف الفريد للكيان.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// تاريخ الإنشاء.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
