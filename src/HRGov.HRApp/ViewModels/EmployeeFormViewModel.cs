using System.ComponentModel.DataAnnotations;
using HRGov.HRApp.Models;

namespace HRGov.HRApp.ViewModels;

public class EmployeeFormViewModel
{
    public int? Id { get; set; }

    [Required]
    [Display(Name = "الاسم الكامل")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "الرقم الوطني")]
    public string NationalId { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    [Display(Name = "تاريخ الميلاد")]
    public DateTime DateOfBirth { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "تاريخ التعيين")]
    public DateTime HireDate { get; set; }

    [Display(Name = "الحالة الوظيفية")]
    public EmploymentStatus Status { get; set; }

    [Display(Name = "رقم الهاتف")]
    public string? PhoneNumber { get; set; }

    [EmailAddress]
    [Display(Name = "البريد الإلكتروني")]
    public string? Email { get; set; }

    [Display(Name = "الدائرة")]
    public int? DepartmentId { get; set; }

    [Display(Name = "المسمى الوظيفي")]
    public int? JobTitleId { get; set; }

    [Display(Name = "الدرجة الوظيفية")]
    public string? Grade { get; set; }

    [Display(Name = "الراتب الأساسي")]
    public decimal BaseSalary { get; set; }

    [Display(Name = "عنوان السكن")]
    public string? Address { get; set; }

    public LookupCollections Lookups { get; set; } = new();
}
