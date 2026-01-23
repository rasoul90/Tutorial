using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Rasad.Domain.Entities;
using Rasad.Infrastructure.Identity;
using Rasad.Web.Models;

namespace Rasad.Web.Controllers;

[Authorize(Roles = "SuperAdmin")]
public class UnitsController : Controller
{
    private readonly ApplicationDbContext _context;

    public UnitsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(Guid? sectionId)
    {
        var sections = await _context.Sections
            .Include(x => x.Department)
            .ThenInclude(x => x.Directorate)
            .ThenInclude(x => x.Ministry)
            .AsNoTracking()
            .OrderBy(x => x.NameAr)
            .ToListAsync();
        ViewBag.SectionId = new SelectList(sections, "Id", "NameAr", sectionId);

        var query = _context.Units
            .Include(x => x.Section)
            .ThenInclude(x => x.Department)
            .ThenInclude(x => x.Directorate)
            .ThenInclude(x => x.Ministry)
            .AsNoTracking();

        if (sectionId.HasValue)
        {
            query = query.Where(x => x.SectionId == sectionId.Value);
        }

        var units = await query.OrderBy(x => x.NameAr).ToListAsync();
        return View(units);
    }

    public async Task<IActionResult> Create(Guid? sectionId)
    {
        await LoadSections(sectionId);
        return View(new UnitFormViewModel { SectionId = sectionId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UnitFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadSections(model.SectionId);
            return View(model);
        }

        var unit = new Unit
        {
            Id = Guid.NewGuid(),
            SectionId = model.SectionId!.Value,
            NameAr = model.NameAr
        };

        _context.Units.Add(unit);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { sectionId = model.SectionId });
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var unit = await _context.Units.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (unit is null)
        {
            return NotFound();
        }

        var model = new UnitFormViewModel
        {
            Id = unit.Id,
            SectionId = unit.SectionId,
            NameAr = unit.NameAr
        };

        await LoadSections(unit.SectionId);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, UnitFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadSections(model.SectionId);
            return View(model);
        }

        var unit = await _context.Units.FindAsync(id);
        if (unit is null)
        {
            return NotFound();
        }

        unit.SectionId = model.SectionId!.Value;
        unit.NameAr = model.NameAr;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { sectionId = model.SectionId });
    }

    public async Task<IActionResult> Delete(Guid id)
    {
        var unit = await _context.Units
            .Include(x => x.Section)
            .ThenInclude(x => x.Department)
            .ThenInclude(x => x.Directorate)
            .ThenInclude(x => x.Ministry)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (unit is null)
        {
            return NotFound();
        }

        return View(unit);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var unit = await _context.Units.FindAsync(id);
        if (unit is null)
        {
            return NotFound();
        }

        _context.Units.Remove(unit);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { sectionId = unit.SectionId });
    }

    private async Task LoadSections(Guid? selectedId)
    {
        var sections = await _context.Sections
            .Include(x => x.Department)
            .ThenInclude(x => x.Directorate)
            .ThenInclude(x => x.Ministry)
            .AsNoTracking()
            .OrderBy(x => x.NameAr)
            .ToListAsync();
        ViewBag.SectionId = new SelectList(sections, "Id", "NameAr", selectedId);
    }
}
