using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRGov.HRApp.Models;

public class LeaveRequest : BaseEntity
{
    [Required]
    [Display(Name = "الموظف")]
    public int EmployeeId { get; set; }

    [ForeignKey(nameof(EmployeeId))]
    public Employee? Employee { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "تاريخ البداية")]
    public DateTime StartDate { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "تاريخ النهاية")]
    public DateTime EndDate { get; set; }

    [Display(Name = "نوع الإجازة")]
    public LeaveType LeaveType { get; set; }

    [Display(Name = "الحالة")]
    public LeaveStatus Status { get; set; }

    [Display(Name = "ملاحظات")]
    public string? Notes { get; set; }

    [Display(Name = "الموافقة من")]
    public int? ApprovedById { get; set; }

    [ForeignKey(nameof(ApprovedById))]
    public Employee? ApprovedBy { get; set; }
}

public enum LeaveType
{
    Annual,
    Sick,
    Unpaid,
    Maternity,
    Emergency
}

public enum LeaveStatus
{
    Pending,
    Approved,
    Rejected,
    Cancelled
}
