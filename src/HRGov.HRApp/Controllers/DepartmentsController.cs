using HRGov.HRApp.Data;
using HRGov.HRApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRGov.HRApp.Controllers;

[Authorize(Roles = "Admin,HRManager")]
public class DepartmentsController : Controller
{
    private readonly ApplicationDbContext _context;

    public DepartmentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var departments = await _context.Departments
            .Include(d => d.Employees)
            .OrderBy(d => d.Name)
            .ToListAsync();
        return View(departments);
    }

    public IActionResult Create()
    {
        return View(new Department());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Department department)
    {
        if (!ModelState.IsValid)
        {
            return View(department);
        }

        _context.Departments.Add(department);
        await _context.SaveChangesAsync();
        TempData["Success"] = "تم إنشاء الدائرة";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var department = await _context.Departments.FindAsync(id);
        if (department == null)
        {
            return NotFound();
        }

        return View(department);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Department department)
    {
        if (id != department.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(department);
        }

        _context.Entry(department).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        TempData["Success"] = "تم تحديث الدائرة";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var department = await _context.Departments.FindAsync(id);
        if (department == null)
        {
            return NotFound();
        }

        _context.Departments.Remove(department);
        await _context.SaveChangesAsync();
        TempData["Success"] = "تم حذف الدائرة";
        return RedirectToAction(nameof(Index));
    }
}
