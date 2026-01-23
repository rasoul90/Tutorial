using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rasad.Domain.Entities;
using Rasad.Infrastructure.Identity;
using Microsoft.Extensions.Localization;
using Rasad.Web.Models;
using Rasad.Web.Resources;

namespace Rasad.Web.Controllers;

[Authorize(Roles = "SuperAdmin")]
public class MinistriesController : Controller
{
    private const int PageSize = 10;
    private readonly ApplicationDbContext _context;
    private readonly IStringLocalizer<SharedResources> _localizer;

    public MinistriesController(ApplicationDbContext context, IStringLocalizer<SharedResources> localizer)
    {
        _context = context;
        _localizer = localizer;
    }

    public async Task<IActionResult> Index(string? search, int page = 1)
    {
        var query = _context.Ministries.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x => x.NameAr.Contains(search) || x.Code.Contains(search));
        }

        var totalCount = await query.CountAsync();
        var ministries = await query
            .OrderBy(x => x.NameAr)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        ViewBag.Search = search ?? string.Empty;
        ViewBag.Page = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize);

        return View(ministries);
    }

    public IActionResult Create()
    {
        return View(new MinistryFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MinistryFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var ministry = new Ministry
        {
            Id = Guid.NewGuid(),
            NameAr = model.NameAr,
            Code = model.Code,
            IsActive = model.IsActive
        };

        _context.Ministries.Add(ministry);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var ministry = await _context.Ministries.FindAsync(id);
        if (ministry is null)
        {
            return NotFound();
        }

        var model = new MinistryFormViewModel
        {
            Id = ministry.Id,
            NameAr = ministry.NameAr,
            Code = ministry.Code,
            IsActive = ministry.IsActive
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, MinistryFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var ministry = await _context.Ministries.FindAsync(id);
        if (ministry is null)
        {
            return NotFound();
        }

        ministry.NameAr = model.NameAr;
        ministry.Code = model.Code;
        ministry.IsActive = model.IsActive;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(Guid id)
    {
        var ministry = await _context.Ministries
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (ministry is null)
        {
            return NotFound();
        }

        return View(ministry);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var ministry = await _context.Ministries.FindAsync(id);
        if (ministry is null)
        {
            return NotFound();
        }

        var hasChildren = await _context.Directorates.AnyAsync(x => x.MinistryId == id)
                          || await _context.MinistryDataWindows.AnyAsync(x => x.MinistryId == id);
        if (hasChildren)
        {
            TempData["ErrorMessage"] = _localizer["MinistryDeleteBlocked"];
            return RedirectToAction(nameof(Delete), new { id });
        }

        _context.Ministries.Remove(ministry);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}
