using HRGov.HRApp.Models;
using HRGov.HRApp.ViewModels;

namespace HRGov.HRApp.Services;

public interface IEmployeeService
{
    Task<Employee?> GetEmployeeAsync(int id);
    Task<IReadOnlyList<EmployeeSummaryViewModel>> GetEmployeesAsync(string? department = null, EmploymentStatus? status = null);
    Task<Employee> CreateEmployeeAsync(EmployeeFormViewModel model);
    Task<Employee?> UpdateEmployeeAsync(int id, EmployeeFormViewModel model);
    Task<bool> DeleteEmployeeAsync(int id);
}
