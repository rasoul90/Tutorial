using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRGov.HRApp.Models;

public class PerformanceReview : BaseEntity
{
    [Display(Name = "الموظف")]
    public int EmployeeId { get; set; }

    [ForeignKey(nameof(EmployeeId))]
    public Employee? Employee { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "تاريخ التقييم")]
    public DateTime ReviewDate { get; set; }

    [Range(1, 5)]
    [Display(Name = "التقييم العام")]
    public int OverallScore { get; set; }

    [Display(Name = "ملاحظات")]
    public string? Comments { get; set; }

    [Display(Name = "خطط التطوير")]
    public string? DevelopmentPlan { get; set; }
}
