using HRGov.HRApp.Data;
using HRGov.HRApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HRGov.HRApp.Controllers;

[Authorize(Roles = "Supervisor,HRManager")]
public class PerformanceReviewsController : Controller
{
    private readonly ApplicationDbContext _context;

    public PerformanceReviewsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var reviews = await _context.PerformanceReviews
            .Include(r => r.Employee)
            .OrderByDescending(r => r.ReviewDate)
            .ToListAsync();
        return View(reviews);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateEmployeesAsync();
        return View(new PerformanceReview
        {
            ReviewDate = DateTime.UtcNow
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PerformanceReview review)
    {
        if (!ModelState.IsValid)
        {
            await PopulateEmployeesAsync();
            return View(review);
        }

        _context.PerformanceReviews.Add(review);
        await _context.SaveChangesAsync();
        TempData["Success"] = "تم إضافة تقييم الأداء";
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateEmployeesAsync()
    {
        ViewBag.Employees = new SelectList(await _context.Employees
            .OrderBy(e => e.FullName)
            .ToListAsync(), "Id", "FullName");
    }
}
