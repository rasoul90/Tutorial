using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rasad.Infrastructure.Identity;

namespace Rasad.Web.Controllers;

[Authorize(Roles = "SuperAdmin")]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var totalMinistries = await _context.Ministries.AsNoTracking().CountAsync();
        var totalEmployees = await _context.Employees.AsNoTracking().CountAsync(x => !x.IsDeleted);
        var totalUnverified = await _context.Employees.AsNoTracking().CountAsync(x => !x.IsDeleted && !x.IsVerified);

        var ghostCount = await _context.Employees.AsNoTracking()
            .Where(x => !x.IsDeleted)
            .GroupBy(x => x.NationalIdHash)
            .Where(g => g.Select(x => x.NormalizedName).Distinct().Count() > 1)
            .CountAsync();

        var duplicateCount = await _context.Employees.AsNoTracking()
            .Where(x => !x.IsDeleted)
            .GroupBy(x => new { x.NationalIdHash, x.NormalizedName })
            .Where(g => g.Select(x => x.MinistryId).Distinct().Count() > 1)
            .CountAsync();

        ViewBag.TotalMinistries = totalMinistries;
        ViewBag.TotalEmployees = totalEmployees;
        ViewBag.TotalUnverified = totalUnverified;
        ViewBag.GhostCount = ghostCount;
        ViewBag.DuplicateCount = duplicateCount;

        return View();
    }
}
