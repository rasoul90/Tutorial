using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRGov.HRApp.Models;

public class Employee : BaseEntity
{
    [Required]
    [MaxLength(200)]
    [Display(Name = "الاسم الكامل")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
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

    [Phone]
    [Display(Name = "رقم الهاتف")]
    public string? PhoneNumber { get; set; }

    [EmailAddress]
    [Display(Name = "البريد الإلكتروني")]
    public string? Email { get; set; }

    [Display(Name = "الدائرة")]
    public int? DepartmentId { get; set; }

    [ForeignKey(nameof(DepartmentId))]
    public Department? Department { get; set; }

    [Display(Name = "المسمى الوظيفي")]
    public int? JobTitleId { get; set; }

    [ForeignKey(nameof(JobTitleId))]
    public JobTitle? JobTitle { get; set; }

    [Display(Name = "الدرجة الوظيفية")]
    public string? Grade { get; set; }

    [Display(Name = "الراتب الأساسي")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal BaseSalary { get; set; }

    [Display(Name = "عنوان السكن")]
    public string? Address { get; set; }

    [Display(Name = "المستمسكات الحكومية")]
    public ICollection<GovernmentDocument> GovernmentDocuments { get; set; } = new List<GovernmentDocument>();

    public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
    public ICollection<PayrollRecord> PayrollRecords { get; set; } = new List<PayrollRecord>();
    public ICollection<PerformanceReview> PerformanceReviews { get; set; } = new List<PerformanceReview>();
    public ICollection<EmployeeTraining> TrainingHistory { get; set; } = new List<EmployeeTraining>();
}

public enum EmploymentStatus
{
    Active,
    OnLeave,
    Suspended,
    Retired,
    Terminated
}
