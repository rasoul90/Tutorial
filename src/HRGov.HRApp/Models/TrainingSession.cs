using System.ComponentModel.DataAnnotations;

namespace HRGov.HRApp.Models;

public class TrainingSession : BaseEntity
{
    [Required]
    [MaxLength(200)]
    [Display(Name = "عنوان الدورة")]
    public string Title { get; set; } = string.Empty;

    [Display(Name = "الجهة التدريبية")]
    public string? Provider { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "تاريخ البداية")]
    public DateTime StartDate { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "تاريخ النهاية")]
    public DateTime EndDate { get; set; }

    [Display(Name = "الموقع")]
    public string? Location { get; set; }

    public ICollection<EmployeeTraining> Participants { get; set; } = new List<EmployeeTraining>();
}

public class EmployeeTraining
{
    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public int TrainingSessionId { get; set; }
    public TrainingSession? TrainingSession { get; set; }

    [Display(Name = "نتيجة التقييم")]
    public string? EvaluationResult { get; set; }
}
