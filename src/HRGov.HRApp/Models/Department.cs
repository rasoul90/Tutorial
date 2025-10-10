using System.ComponentModel.DataAnnotations;

namespace HRGov.HRApp.Models;

public class Department : BaseEntity
{
    [Required]
    [MaxLength(150)]
    [Display(Name = "اسم الدائرة")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "وصف")]
    public string? Description { get; set; }

    [Display(Name = "الموقع")]
    public string? Location { get; set; }

    [Display(Name = "المدير")]
    public string? DirectorName { get; set; }

    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
