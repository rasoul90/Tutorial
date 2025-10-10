using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRGov.HRApp.Models;

public class GovernmentDocument : BaseEntity
{
    [Required]
    [Display(Name = "اسم المستمسك")]
    public string Title { get; set; } = string.Empty;

    [Display(Name = "رقم المستمسك")]
    public string? DocumentNumber { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "تاريخ الإصدار")]
    public DateTime IssueDate { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "تاريخ الانتهاء")]
    public DateTime? ExpiryDate { get; set; }

    [Display(Name = "الموظف")]
    public int EmployeeId { get; set; }

    [ForeignKey(nameof(EmployeeId))]
    public Employee? Employee { get; set; }
}
