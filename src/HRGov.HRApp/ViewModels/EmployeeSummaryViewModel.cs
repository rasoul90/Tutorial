using HRGov.HRApp.Models;

namespace HRGov.HRApp.ViewModels;

public class EmployeeSummaryViewModel
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public EmploymentStatus Status { get; set; }
    public DateTime HireDate { get; set; }
    public decimal BaseSalary { get; set; }
}
