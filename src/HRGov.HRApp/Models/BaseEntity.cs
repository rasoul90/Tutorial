using System.ComponentModel.DataAnnotations;

namespace HRGov.HRApp.Models;

public abstract class BaseEntity
{
    [Key]
    public int Id { get; set; }

    [Display(Name = "تاريخ الإنشاء")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Display(Name = "آخر تحديث")]
    public DateTime? UpdatedAt { get; set; }
}
