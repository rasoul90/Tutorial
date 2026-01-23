using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Rasad.Domain.Entities;
using Rasad.Infrastructure.Identity;
using Rasad.Web.Models;

namespace Rasad.Web.Controllers;

[Authorize(Roles = "SuperAdmin")]
public class MinistryDataWindowsController : Controller
{
    private readonly ApplicationDbContext _context;

    public MinistryDataWindowsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(Guid? ministryId)
    {
        var ministries = await _context.Ministries.AsNoTracking().OrderBy(x => x.NameAr).ToListAsync();
        ViewBag.MinistryId = new SelectList(ministries, "Id", "NameAr", ministryId);

        var query = _context.MinistryDataWindows
            .Include(x => x.Ministry)
            .AsNoTracking();

        if (ministryId.HasValue)
        {
            query = query.Where(x => x.MinistryId == ministryId.Value);
        }

        var windows = await query.OrderByDescending(x => x.StartAt).ToListAsync();
        return View(windows);
    }

    public async Task<IActionResult> Create(Guid? ministryId)
    {
        await LoadMinistries(ministryId);
        return View(new MinistryDataWindowFormViewModel { MinistryId = ministryId, StartAt = DateTime.UtcNow, EndAt = DateTime.UtcNow.AddDays(1) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MinistryDataWindowFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadMinistries(model.MinistryId);
            return View(model);
        }

        var window = new MinistryDataWindow
        {
            Id = Guid.NewGuid(),
            MinistryId = model.MinistryId!.Value,
            StartAt = model.StartAt,
            EndAt = model.EndAt,
            IsActive = model.IsActive,
            AfterCloseMessageAr = model.AfterCloseMessageAr,
            CreatedByUserId = User.Identity?.Name ?? string.Empty,
            CreatedAt = DateTime.UtcNow
        };

        _context.MinistryDataWindows.Add(window);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { ministryId = model.MinistryId });
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var window = await _context.MinistryDataWindows.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (window is null)
        {
            return NotFound();
        }

        var model = new MinistryDataWindowFormViewModel
        {
            Id = window.Id,
            MinistryId = window.MinistryId,
            StartAt = window.StartAt,
            EndAt = window.EndAt,
            IsActive = window.IsActive,
            AfterCloseMessageAr = window.AfterCloseMessageAr
        };

        await LoadMinistries(window.MinistryId);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, MinistryDataWindowFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadMinistries(model.MinistryId);
            return View(model);
        }

        var window = await _context.MinistryDataWindows.FindAsync(id);
        if (window is null)
        {
            return NotFound();
        }

        window.MinistryId = model.MinistryId!.Value;
        window.StartAt = model.StartAt;
        window.EndAt = model.EndAt;
        window.IsActive = model.IsActive;
        window.AfterCloseMessageAr = model.AfterCloseMessageAr;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { ministryId = model.MinistryId });
    }

    public async Task<IActionResult> Delete(Guid id)
    {
        var window = await _context.MinistryDataWindows
            .Include(x => x.Ministry)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (window is null)
        {
            return NotFound();
        }

        return View(window);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var window = await _context.MinistryDataWindows.FindAsync(id);
        if (window is null)
        {
            return NotFound();
        }

        _context.MinistryDataWindows.Remove(window);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { ministryId = window.MinistryId });
    }

    private async Task LoadMinistries(Guid? selectedId)
    {
        var ministries = await _context.Ministries.AsNoTracking().OrderBy(x => x.NameAr).ToListAsync();
        ViewBag.MinistryId = new SelectList(ministries, "Id", "NameAr", selectedId);
    }
}
