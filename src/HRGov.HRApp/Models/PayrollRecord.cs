using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRGov.HRApp.Models;

public class PayrollRecord : BaseEntity
{
    [Display(Name = "الموظف")]
    public int EmployeeId { get; set; }

    [ForeignKey(nameof(EmployeeId))]
    public Employee? Employee { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "تاريخ الراتب")]
    public DateTime PayrollDate { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "الراتب الأساسي")]
    public decimal BasicSalary { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "البدلات")]
    public decimal Allowances { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "الاستقطاعات")]
    public decimal Deductions { get; set; }

    [NotMapped]
    public decimal NetSalary => BasicSalary + Allowances - Deductions;
}
