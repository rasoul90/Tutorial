using HRGov.HRApp.Data;
using HRGov.HRApp.Models;
using HRGov.HRApp.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace HRGov.HRApp.Services;

public class EmployeeService : IEmployeeService
{
    private readonly ApplicationDbContext _context;

    public EmployeeService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Employee?> GetEmployeeAsync(int id)
    {
        return await _context.Employees
            .Include(e => e.Department)
            .Include(e => e.JobTitle)
            .Include(e => e.GovernmentDocuments)
            .Include(e => e.LeaveRequests)
            .Include(e => e.PerformanceReviews)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IReadOnlyList<EmployeeSummaryViewModel>> GetEmployeesAsync(string? department = null, EmploymentStatus? status = null)
    {
        var query = _context.Employees
            .Include(e => e.Department)
            .Include(e => e.JobTitle)
            .AsQueryable();

        if (!string.IsNullOrEmpty(department))
        {
            query = query.Where(e => e.Department != null && e.Department.Name == department);
        }

        if (status.HasValue)
        {
            query = query.Where(e => e.Status == status);
        }

        return await query
            .OrderBy(e => e.FullName)
            .Select(e => new EmployeeSummaryViewModel
            {
                Id = e.Id,
                FullName = e.FullName,
                Department = e.Department != null ? e.Department.Name : "غير محدد",
                JobTitle = e.JobTitle != null ? e.JobTitle.Name : "غير محدد",
                Status = e.Status,
                HireDate = e.HireDate,
                BaseSalary = e.BaseSalary
            })
            .ToListAsync();
    }

    public async Task<Employee> CreateEmployeeAsync(EmployeeFormViewModel model)
    {
        var employee = new Employee
        {
            FullName = model.FullName,
            NationalId = model.NationalId,
            DateOfBirth = model.DateOfBirth,
            HireDate = model.HireDate,
            Status = model.Status,
            PhoneNumber = model.PhoneNumber,
            Email = model.Email,
            DepartmentId = model.DepartmentId,
            JobTitleId = model.JobTitleId,
            Grade = model.Grade,
            BaseSalary = model.BaseSalary,
            Address = model.Address
        };

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();
        return employee;
    }

    public async Task<Employee?> UpdateEmployeeAsync(int id, EmployeeFormViewModel model)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null)
        {
            return null;
        }

        employee.FullName = model.FullName;
        employee.NationalId = model.NationalId;
        employee.DateOfBirth = model.DateOfBirth;
        employee.HireDate = model.HireDate;
        employee.Status = model.Status;
        employee.PhoneNumber = model.PhoneNumber;
        employee.Email = model.Email;
        employee.DepartmentId = model.DepartmentId;
        employee.JobTitleId = model.JobTitleId;
        employee.Grade = model.Grade;
        employee.BaseSalary = model.BaseSalary;
        employee.Address = model.Address;
        employee.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return employee;
    }

    public async Task<bool> DeleteEmployeeAsync(int id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null)
        {
            return false;
        }

        _context.Employees.Remove(employee);
        await _context.SaveChangesAsync();
        return true;
    }
}
