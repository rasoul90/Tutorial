using HRGov.HRApp.Models;
using HRGov.HRApp.Services;
using HRGov.HRApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRGov.HRApp.Controllers;

[Authorize]
public class EmployeesController : Controller
{
    private readonly IEmployeeService _employeeService;
    private readonly ILookupService _lookupService;

    public EmployeesController(IEmployeeService employeeService, ILookupService lookupService)
    {
        _employeeService = employeeService;
        _lookupService = lookupService;
    }

    public async Task<IActionResult> Index(string? department, EmploymentStatus? status)
    {
        var employees = await _employeeService.GetEmployeesAsync(department, status);
        ViewBag.DepartmentFilter = department;
        ViewBag.StatusFilter = status;
        return View(employees);
    }

    public async Task<IActionResult> Details(int id)
    {
        var employee = await _employeeService.GetEmployeeAsync(id);
        if (employee == null)
        {
            return NotFound();
        }

        return View(employee);
    }

    public async Task<IActionResult> Create()
    {
        var lookups = await _lookupService.GetLookupsAsync();
        return View(new EmployeeFormViewModel
        {
            HireDate = DateTime.UtcNow,
            Status = EmploymentStatus.Active,
            Lookups = lookups
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EmployeeFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Lookups = await _lookupService.GetLookupsAsync();
            return View(model);
        }

        await _employeeService.CreateEmployeeAsync(model);
        TempData["Success"] = "تم إضافة الموظف بنجاح";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var employee = await _employeeService.GetEmployeeAsync(id);
        if (employee == null)
        {
            return NotFound();
        }

        var lookups = await _lookupService.GetLookupsAsync();
        var model = new EmployeeFormViewModel
        {
            Id = employee.Id,
            FullName = employee.FullName,
            NationalId = employee.NationalId,
            DateOfBirth = employee.DateOfBirth,
            HireDate = employee.HireDate,
            Status = employee.Status,
            PhoneNumber = employee.PhoneNumber,
            Email = employee.Email,
            DepartmentId = employee.DepartmentId,
            JobTitleId = employee.JobTitleId,
            Grade = employee.Grade,
            BaseSalary = employee.BaseSalary,
            Address = employee.Address,
            Lookups = lookups
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EmployeeFormViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            model.Lookups = await _lookupService.GetLookupsAsync();
            return View(model);
        }

        var updated = await _employeeService.UpdateEmployeeAsync(id, model);
        if (updated == null)
        {
            return NotFound();
        }

        TempData["Success"] = "تم تحديث بيانات الموظف";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _employeeService.DeleteEmployeeAsync(id);
        if (!success)
        {
            return NotFound();
        }

        TempData["Success"] = "تم حذف الموظف";
        return RedirectToAction(nameof(Index));
    }
}
