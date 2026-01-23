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
public class DepartmentsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IStringLocalizer<SharedResources> _localizer;

    public DepartmentsController(ApplicationDbContext context, IStringLocalizer<SharedResources> localizer)
    {
        _context = context;
        _localizer = localizer;
    }

    public async Task<IActionResult> Index(Guid? directorateId)
    {
        var directorates = await _context.Directorates
            .Include(x => x.Ministry)
            .AsNoTracking()
            .OrderBy(x => x.NameAr)
            .ToListAsync();
        ViewBag.DirectorateId = new SelectList(directorates, "Id", "NameAr", directorateId);

        var query = _context.Departments
            .Include(x => x.Directorate)
            .ThenInclude(x => x.Ministry)
            .AsNoTracking();

        if (directorateId.HasValue)
        {
            query = query.Where(x => x.DirectorateId == directorateId.Value);
        }

        var departments = await query.OrderBy(x => x.NameAr).ToListAsync();
        return View(departments);
    }

    public async Task<IActionResult> Create(Guid? directorateId)
    {
        await LoadDirectorates(directorateId);
        return View(new DepartmentFormViewModel { DirectorateId = directorateId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DepartmentFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadDirectorates(model.DirectorateId);
            return View(model);
        }

        var department = new Department
        {
            Id = Guid.NewGuid(),
            DirectorateId = model.DirectorateId!.Value,
            NameAr = model.NameAr
        };

        _context.Departments.Add(department);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { directorateId = model.DirectorateId });
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var department = await _context.Departments.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (department is null)
        {
            return NotFound();
        }

        var model = new DepartmentFormViewModel
        {
            Id = department.Id,
            DirectorateId = department.DirectorateId,
            NameAr = department.NameAr
        };

        await LoadDirectorates(department.DirectorateId);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, DepartmentFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadDirectorates(model.DirectorateId);
            return View(model);
        }

        var department = await _context.Departments.FindAsync(id);
        if (department is null)
        {
            return NotFound();
        }

        department.DirectorateId = model.DirectorateId!.Value;
        department.NameAr = model.NameAr;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { directorateId = model.DirectorateId });
    }

    public async Task<IActionResult> Delete(Guid id)
    {
        var department = await _context.Departments
            .Include(x => x.Directorate)
            .ThenInclude(x => x.Ministry)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (department is null)
        {
            return NotFound();
        }

        return View(department);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var department = await _context.Departments.FindAsync(id);
        if (department is null)
        {
            return NotFound();
        }

        var hasChildren = await _context.Sections.AnyAsync(x => x.DepartmentId == id);
        if (hasChildren)
        {
            TempData["ErrorMessage"] = _localizer["DepartmentDeleteBlocked"];
            return RedirectToAction(nameof(Delete), new { id });
        }

        _context.Departments.Remove(department);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { directorateId = department.DirectorateId });
    }

    private async Task LoadDirectorates(Guid? selectedId)
    {
        var directorates = await _context.Directorates
            .Include(x => x.Ministry)
            .AsNoTracking()
            .OrderBy(x => x.NameAr)
            .ToListAsync();
        ViewBag.DirectorateId = new SelectList(directorates, "Id", "NameAr", selectedId);
    }
}
