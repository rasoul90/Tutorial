using System.ComponentModel.DataAnnotations;

namespace HRGov.HRApp.Models;

public class JobTitle : BaseEntity
{
    [Required]
    [MaxLength(150)]
    [Display(Name = "المسمى الوظيفي")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "الوصف")]
    public string? Description { get; set; }

    [Display(Name = "السلم الوظيفي")]
    public string? Grade { get; set; }

    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
