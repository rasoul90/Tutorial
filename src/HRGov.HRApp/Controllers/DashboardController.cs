using HRGov.HRApp.Data;
using HRGov.HRApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRGov.HRApp.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var totalEmployees = await _context.Employees.CountAsync();
        var activeEmployees = await _context.Employees.CountAsync(e => e.Status == Models.EmploymentStatus.Active);
        var pendingLeaveRequests = await _context.LeaveRequests.CountAsync(l => l.Status == Models.LeaveStatus.Pending);
        var totalPayroll = await _context.PayrollRecords
            .Where(p => p.PayrollDate.Month == DateTime.UtcNow.Month && p.PayrollDate.Year == DateTime.UtcNow.Year)
            .SumAsync(p => p.BasicSalary + p.Allowances - p.Deductions);

        var departments = await _context.Departments
            .Select(d => new DepartmentSummary
            {
                Name = d.Name,
                EmployeeCount = d.Employees.Count,
                AverageSalary = d.Employees.Any() ? d.Employees.Average(e => e.BaseSalary) : 0
            })
            .ToListAsync();

        var model = new DashboardViewModel
        {
            TotalEmployees = totalEmployees,
            ActiveEmployees = activeEmployees,
            PendingLeaveRequests = pendingLeaveRequests,
            TotalMonthlyPayroll = totalPayroll,
            Departments = departments
        };

        return View(model);
    }
}
