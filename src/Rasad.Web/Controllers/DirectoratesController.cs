using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Rasad.Domain.Entities;
using Rasad.Infrastructure.Identity;
using Rasad.Web.Models;
using Rasad.Web.Resources;

namespace Rasad.Web.Controllers;

[Authorize(Roles = "SuperAdmin")]
public class DirectoratesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IStringLocalizer<SharedResources> _localizer;

    public DirectoratesController(ApplicationDbContext context, IStringLocalizer<SharedResources> localizer)
    {
        _context = context;
        _localizer = localizer;
    }

    public async Task<IActionResult> Index(Guid? ministryId)
    {
        var ministries = await _context.Ministries.AsNoTracking().OrderBy(x => x.NameAr).ToListAsync();
        ViewBag.MinistryId = new SelectList(ministries, "Id", "NameAr", ministryId);

        var query = _context.Directorates
            .Include(x => x.Ministry)
            .AsNoTracking();

        if (ministryId.HasValue)
        {
            query = query.Where(x => x.MinistryId == ministryId.Value);
        }

        var directorates = await query.OrderBy(x => x.NameAr).ToListAsync();
        return View(directorates);
    }

    public async Task<IActionResult> Create(Guid? ministryId)
    {
        await LoadMinistries(ministryId);
        return View(new DirectorateFormViewModel { MinistryId = ministryId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DirectorateFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadMinistries(model.MinistryId);
            return View(model);
        }

        var directorate = new Directorate
        {
            Id = Guid.NewGuid(),
            MinistryId = model.MinistryId!.Value,
            NameAr = model.NameAr
        };

        _context.Directorates.Add(directorate);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { ministryId = model.MinistryId });
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var directorate = await _context.Directorates.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (directorate is null)
        {
            return NotFound();
        }

        var model = new DirectorateFormViewModel
        {
            Id = directorate.Id,
            MinistryId = directorate.MinistryId,
            NameAr = directorate.NameAr
        };

        await LoadMinistries(directorate.MinistryId);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, DirectorateFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadMinistries(model.MinistryId);
            return View(model);
        }

        var directorate = await _context.Directorates.FindAsync(id);
        if (directorate is null)
        {
            return NotFound();
        }

        directorate.MinistryId = model.MinistryId!.Value;
        directorate.NameAr = model.NameAr;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { ministryId = model.MinistryId });
    }

    public async Task<IActionResult> Delete(Guid id)
    {
        var directorate = await _context.Directorates
            .Include(x => x.Ministry)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (directorate is null)
        {
            return NotFound();
        }

        return View(directorate);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var directorate = await _context.Directorates.FindAsync(id);
        if (directorate is null)
        {
            return NotFound();
        }

        var hasChildren = await _context.Departments.AnyAsync(x => x.DirectorateId == id);
        if (hasChildren)
        {
            TempData["ErrorMessage"] = _localizer["DirectorateDeleteBlocked"];
            return RedirectToAction(nameof(Delete), new { id });
        }

        _context.Directorates.Remove(directorate);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { ministryId = directorate.MinistryId });
    }

    private async Task LoadMinistries(Guid? selectedId)
    {
        var ministries = await _context.Ministries.AsNoTracking().OrderBy(x => x.NameAr).ToListAsync();
        ViewBag.MinistryId = new SelectList(ministries, "Id", "NameAr", selectedId);
    }
}
