namespace HRGov.HRApp.ViewModels;

public class DashboardViewModel
{
    public int TotalEmployees { get; set; }
    public int ActiveEmployees { get; set; }
    public int PendingLeaveRequests { get; set; }
    public decimal TotalMonthlyPayroll { get; set; }
    public IReadOnlyCollection<DepartmentSummary> Departments { get; set; } = Array.Empty<DepartmentSummary>();
}

public class DepartmentSummary
{
    public string Name { get; set; } = string.Empty;
    public int EmployeeCount { get; set; }
    public decimal AverageSalary { get; set; }
}
