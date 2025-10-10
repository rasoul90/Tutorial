using HRGov.HRApp.Data;
using HRGov.HRApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HRGov.HRApp.Controllers;

[Authorize(Roles = "Finance,HRManager")]
public class PayrollController : Controller
{
    private readonly ApplicationDbContext _context;

    public PayrollController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int? month, int? year)
    {
        var payrolls = _context.PayrollRecords
            .Include(p => p.Employee)
            .AsQueryable();

        if (month.HasValue)
        {
            payrolls = payrolls.Where(p => p.PayrollDate.Month == month);
        }

        if (year.HasValue)
        {
            payrolls = payrolls.Where(p => p.PayrollDate.Year == year);
        }

        var list = await payrolls
            .OrderByDescending(p => p.PayrollDate)
            .ToListAsync();

        ViewBag.Month = month;
        ViewBag.Year = year;

        return View(list);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateEmployeesAsync();
        return View(new PayrollRecord
        {
            PayrollDate = DateTime.UtcNow
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PayrollRecord record)
    {
        if (!ModelState.IsValid)
        {
            await PopulateEmployeesAsync();
            return View(record);
        }

        _context.PayrollRecords.Add(record);
        await _context.SaveChangesAsync();
        TempData["Success"] = "تم إنشاء قيد الرواتب";
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateEmployeesAsync()
    {
        ViewBag.Employees = new SelectList(await _context.Employees
            .OrderBy(e => e.FullName)
            .ToListAsync(), "Id", "FullName");
    }
}
