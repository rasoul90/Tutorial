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
public class SectionsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IStringLocalizer<SharedResources> _localizer;

    public SectionsController(ApplicationDbContext context, IStringLocalizer<SharedResources> localizer)
    {
        _context = context;
        _localizer = localizer;
    }

    public async Task<IActionResult> Index(Guid? departmentId)
    {
        var departments = await _context.Departments
            .Include(x => x.Directorate)
            .ThenInclude(x => x.Ministry)
            .AsNoTracking()
            .OrderBy(x => x.NameAr)
            .ToListAsync();
        ViewBag.DepartmentId = new SelectList(departments, "Id", "NameAr", departmentId);

        var query = _context.Sections
            .Include(x => x.Department)
            .ThenInclude(x => x.Directorate)
            .ThenInclude(x => x.Ministry)
            .AsNoTracking();

        if (departmentId.HasValue)
        {
            query = query.Where(x => x.DepartmentId == departmentId.Value);
        }

        var sections = await query.OrderBy(x => x.NameAr).ToListAsync();
        return View(sections);
    }

    public async Task<IActionResult> Create(Guid? departmentId)
    {
        await LoadDepartments(departmentId);
        return View(new SectionFormViewModel { DepartmentId = departmentId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SectionFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadDepartments(model.DepartmentId);
            return View(model);
        }

        var section = new Section
        {
            Id = Guid.NewGuid(),
            DepartmentId = model.DepartmentId!.Value,
            NameAr = model.NameAr
        };

        _context.Sections.Add(section);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { departmentId = model.DepartmentId });
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var section = await _context.Sections.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (section is null)
        {
            return NotFound();
        }

        var model = new SectionFormViewModel
        {
            Id = section.Id,
            DepartmentId = section.DepartmentId,
            NameAr = section.NameAr
        };

        await LoadDepartments(section.DepartmentId);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, SectionFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadDepartments(model.DepartmentId);
            return View(model);
        }

        var section = await _context.Sections.FindAsync(id);
        if (section is null)
        {
            return NotFound();
        }

        section.DepartmentId = model.DepartmentId!.Value;
        section.NameAr = model.NameAr;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { departmentId = model.DepartmentId });
    }

    public async Task<IActionResult> Delete(Guid id)
    {
        var section = await _context.Sections
            .Include(x => x.Department)
            .ThenInclude(x => x.Directorate)
            .ThenInclude(x => x.Ministry)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (section is null)
        {
            return NotFound();
        }

        return View(section);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var section = await _context.Sections.FindAsync(id);
        if (section is null)
        {
            return NotFound();
        }

        var hasChildren = await _context.Units.AnyAsync(x => x.SectionId == id);
        if (hasChildren)
        {
            TempData["ErrorMessage"] = _localizer["SectionDeleteBlocked"];
            return RedirectToAction(nameof(Delete), new { id });
        }

        _context.Sections.Remove(section);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { departmentId = section.DepartmentId });
    }

    private async Task LoadDepartments(Guid? selectedId)
    {
        var departments = await _context.Departments
            .Include(x => x.Directorate)
            .ThenInclude(x => x.Ministry)
            .AsNoTracking()
            .OrderBy(x => x.NameAr)
            .ToListAsync();
        ViewBag.DepartmentId = new SelectList(departments, "Id", "NameAr", selectedId);
    }
}
