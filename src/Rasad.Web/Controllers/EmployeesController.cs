using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Rasad.Application.Services;
using Rasad.Domain.Entities;
using Rasad.Infrastructure.Identity;
using Rasad.Infrastructure.Security;
using Rasad.Web.Models.Employees;
using Rasad.Web.Resources;

namespace Rasad.Web.Controllers;

[Authorize]
public class EmployeesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IEncryptionService _encryptionService;
    private readonly IStringLocalizer<SharedResources> _localizer;
    private readonly UserManager<ApplicationUser> _userManager;

    public EmployeesController(ApplicationDbContext context, IEncryptionService encryptionService, IStringLocalizer<SharedResources> localizer, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _encryptionService = encryptionService;
        _localizer = localizer;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var scope = await GetScopeAsync();
        var query = _context.Employees
            .Include(x => x.Ministry)
            .Include(x => x.Directorate)
            .Include(x => x.Department)
            .Include(x => x.Section)
            .Include(x => x.Unit)
            .AsNoTracking()
            .Where(x => !x.IsDeleted);

        if (!User.IsInRole("SuperAdmin") && scope is not null)
        {
            query = ApplyScope(query, scope);
        }

        var employees = await query.OrderBy(x => x.FullName).ToListAsync();
        var canViewFull = await CanViewNationalIdFullAsync(scope);

        var list = employees.Select(e => new EmployeeListItemViewModel
        {
            Id = e.Id,
            FullName = e.FullName,
            JobNumber = e.JobNumber,
            NationalIdDisplay = FormatNationalId(e.NationalIdEncrypted, canViewFull),
            MinistryName = e.Ministry?.NameAr ?? string.Empty,
            DirectorateName = e.Directorate?.NameAr ?? string.Empty,
            DepartmentName = e.Department?.NameAr,
            SectionName = e.Section?.NameAr,
            UnitName = e.Unit?.NameAr
        }).ToList();

        ViewBag.IsWindowOpen = await IsWindowOpenAsync(scope);
        ViewBag.WindowMessage = await GetWindowMessageAsync(scope);

        return View(list);
    }

    public async Task<IActionResult> Create()
    {
        var scope = await GetScopeAsync();
        if (!await EnsureWindowOpenAsync(scope))
        {
            TempData["ErrorMessage"] = await GetWindowMessageAsync(scope);
            return RedirectToAction(nameof(Index));
        }

        var model = new EmployeeFormViewModel();
        await LoadLookupsAsync(scope, model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EmployeeFormViewModel model)
    {
        var scope = await GetScopeAsync();
        if (!await EnsureWindowOpenAsync(scope))
        {
            TempData["ErrorMessage"] = await GetWindowMessageAsync(scope);
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            await LoadLookupsAsync(scope, model);
            return View(model);
        }

        if (!await ValidateScopeSelectionAsync(scope, model))
        {
            ModelState.AddModelError(string.Empty, _localizer["ScopeViolationMessage"]);
            await LoadLookupsAsync(scope, model);
            return View(model);
        }

        if (!await ValidateHierarchyAsync(model))
        {
            ModelState.AddModelError(string.Empty, _localizer["HierarchyViolationMessage"]);
            await LoadLookupsAsync(scope, model);
            return View(model);
        }

        var normalizedName = NameNormalizer.Normalize(model.FullName);
        var nationalIdHash = _encryptionService.ComputeHash(model.NationalId);

        var duplicate = await _context.Employees.AnyAsync(e => !e.IsDeleted
            && e.MinistryId == model.MinistryId
            && e.NationalIdHash == nationalIdHash);
        if (duplicate)
        {
            ModelState.AddModelError(nameof(EmployeeFormViewModel.NationalId), _localizer["DuplicateNationalIdMessage"]);
            await LoadLookupsAsync(scope, model);
            return View(model);
        }

        var jobNumberDuplicate = await _context.Employees.AnyAsync(e => !e.IsDeleted
            && e.MinistryId == model.MinistryId
            && e.JobNumber == model.JobNumber);
        if (jobNumberDuplicate)
        {
            ModelState.AddModelError(nameof(EmployeeFormViewModel.JobNumber), _localizer["DuplicateJobNumberMessage"]);
            await LoadLookupsAsync(scope, model);
            return View(model);
        }

        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            FullName = model.FullName,
            NormalizedName = normalizedName,
            JobNumber = model.JobNumber,
            NationalIdEncrypted = _encryptionService.Encrypt(model.NationalId),
            NationalIdHash = nationalIdHash,
            MinistryId = model.MinistryId!.Value,
            DirectorateId = model.DirectorateId!.Value,
            DepartmentId = model.DepartmentId,
            SectionId = model.SectionId,
            UnitId = model.UnitId,
            CreatedByUserId = _userManager.GetUserId(User) ?? string.Empty,
            UpdatedByUserId = _userManager.GetUserId(User) ?? string.Empty,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var scope = await GetScopeAsync();
        if (!await EnsureWindowOpenAsync(scope))
        {
            TempData["ErrorMessage"] = await GetWindowMessageAsync(scope);
            return RedirectToAction(nameof(Index));
        }

        var employee = await _context.Employees.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (employee is null)
        {
            return NotFound();
        }

        if (!User.IsInRole("SuperAdmin") && scope is not null)
        {
            var scopeQuery = ApplyScope(_context.Employees.AsNoTracking(), scope);
            var hasAccess = await scopeQuery.AnyAsync(x => x.Id == id);
            if (!hasAccess)
            {
                return Forbid();
            }
        }

        var model = new EmployeeFormViewModel
        {
            Id = employee.Id,
            FullName = employee.FullName,
            JobNumber = employee.JobNumber,
            NationalId = _encryptionService.Decrypt(employee.NationalIdEncrypted),
            MinistryId = employee.MinistryId,
            DirectorateId = employee.DirectorateId,
            DepartmentId = employee.DepartmentId,
            SectionId = employee.SectionId,
            UnitId = employee.UnitId
        };

        await LoadLookupsAsync(scope, model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, EmployeeFormViewModel model)
    {
        var scope = await GetScopeAsync();
        if (!await EnsureWindowOpenAsync(scope))
        {
            TempData["ErrorMessage"] = await GetWindowMessageAsync(scope);
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            await LoadLookupsAsync(scope, model);
            return View(model);
        }

        var employee = await _context.Employees.FindAsync(id);
        if (employee is null)
        {
            return NotFound();
        }

        if (!User.IsInRole("SuperAdmin") && scope is not null)
        {
            var scopeQuery = ApplyScope(_context.Employees.AsNoTracking(), scope);
            var hasAccess = await scopeQuery.AnyAsync(x => x.Id == id);
            if (!hasAccess)
            {
                return Forbid();
            }
        }

        if (!await ValidateScopeSelectionAsync(scope, model))
        {
            ModelState.AddModelError(string.Empty, _localizer["ScopeViolationMessage"]);
            await LoadLookupsAsync(scope, model);
            return View(model);
        }

        if (!await ValidateHierarchyAsync(model))
        {
            ModelState.AddModelError(string.Empty, _localizer["HierarchyViolationMessage"]);
            await LoadLookupsAsync(scope, model);
            return View(model);
        }

        var normalizedName = NameNormalizer.Normalize(model.FullName);
        var nationalIdHash = _encryptionService.ComputeHash(model.NationalId);

        var duplicate = await _context.Employees.AnyAsync(e => e.Id != id && !e.IsDeleted
            && e.MinistryId == model.MinistryId
            && e.NationalIdHash == nationalIdHash);
        if (duplicate)
        {
            ModelState.AddModelError(nameof(EmployeeFormViewModel.NationalId), _localizer["DuplicateNationalIdMessage"]);
            await LoadLookupsAsync(scope, model);
            return View(model);
        }

        var jobNumberDuplicate = await _context.Employees.AnyAsync(e => e.Id != id && !e.IsDeleted
            && e.MinistryId == model.MinistryId
            && e.JobNumber == model.JobNumber);
        if (jobNumberDuplicate)
        {
            ModelState.AddModelError(nameof(EmployeeFormViewModel.JobNumber), _localizer["DuplicateJobNumberMessage"]);
            await LoadLookupsAsync(scope, model);
            return View(model);
        }

        employee.FullName = model.FullName;
        employee.NormalizedName = normalizedName;
        employee.JobNumber = model.JobNumber;
        employee.NationalIdEncrypted = _encryptionService.Encrypt(model.NationalId);
        employee.NationalIdHash = nationalIdHash;
        employee.MinistryId = model.MinistryId!.Value;
        employee.DirectorateId = model.DirectorateId!.Value;
        employee.DepartmentId = model.DepartmentId;
        employee.SectionId = model.SectionId;
        employee.UnitId = model.UnitId;
        employee.UpdatedByUserId = _userManager.GetUserId(User) ?? string.Empty;
        employee.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(Guid id)
    {
        var scope = await GetScopeAsync();
        if (!await EnsureWindowOpenAsync(scope))
        {
            TempData["ErrorMessage"] = await GetWindowMessageAsync(scope);
            return RedirectToAction(nameof(Index));
        }

        var employee = await _context.Employees
            .Include(x => x.Ministry)
            .Include(x => x.Directorate)
            .Include(x => x.Department)
            .Include(x => x.Section)
            .Include(x => x.Unit)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
        if (employee is null)
        {
            return NotFound();
        }

        if (!User.IsInRole("SuperAdmin") && scope is not null)
        {
            var scopeQuery = ApplyScope(_context.Employees.AsNoTracking(), scope);
            var hasAccess = await scopeQuery.AnyAsync(x => x.Id == id);
            if (!hasAccess)
            {
                return Forbid();
            }
        }

        ViewBag.NationalIdDisplay = FormatNationalId(employee.NationalIdEncrypted, await CanViewNationalIdFullAsync(scope));
        return View(employee);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var scope = await GetScopeAsync();
        if (!await EnsureWindowOpenAsync(scope))
        {
            TempData["ErrorMessage"] = await GetWindowMessageAsync(scope);
            return RedirectToAction(nameof(Index));
        }

        if (!User.IsInRole("SuperAdmin") && scope is not null)
        {
            var scopeQuery = ApplyScope(_context.Employees.AsNoTracking(), scope);
            var hasAccess = await scopeQuery.AnyAsync(x => x.Id == id);
            if (!hasAccess)
            {
                return Forbid();
            }
        }

        var employee = await _context.Employees.FindAsync(id);
        if (employee is null)
        {
            return NotFound();
        }

        employee.IsDeleted = true;
        employee.DeletedAt = DateTime.UtcNow;
        employee.DeletedByUserId = _userManager.GetUserId(User) ?? string.Empty;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    private async Task<UserScope?> GetScopeAsync()
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        return await _context.UserScopes
            .Include(x => x.Ministry)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId);
    }

    private static IQueryable<Employee> ApplyScope(IQueryable<Employee> query, UserScope scope)
    {
        query = query.Where(x => x.MinistryId == scope.MinistryId && x.DirectorateId == scope.DirectorateId);

        if (scope.DepartmentId.HasValue)
        {
            query = query.Where(x => x.DepartmentId == scope.DepartmentId);
        }

        if (scope.SectionId.HasValue)
        {
            query = query.Where(x => x.SectionId == scope.SectionId);
        }

        if (scope.UnitId.HasValue)
        {
            query = query.Where(x => x.UnitId == scope.UnitId);
        }

        return query;
    }

    private async Task<bool> EnsureWindowOpenAsync(UserScope? scope)
    {
        if (User.IsInRole("SuperAdmin"))
        {
            return true;
        }

        return await IsWindowOpenAsync(scope);
    }

    private async Task<bool> IsWindowOpenAsync(UserScope? scope)
    {
        if (scope is null)
        {
            return false;
        }

        var now = DateTime.UtcNow;
        return await _context.MinistryDataWindows.AnyAsync(x => x.MinistryId == scope.MinistryId
            && x.IsActive
            && x.StartAt <= now
            && x.EndAt >= now);
    }

    private async Task<string> GetWindowMessageAsync(UserScope? scope)
    {
        if (scope is null)
        {
            return _localizer["WindowClosedMessage"];
        }

        var now = DateTime.UtcNow;
        var message = await _context.MinistryDataWindows
            .Where(x => x.MinistryId == scope.MinistryId)
            .OrderByDescending(x => x.EndAt)
            .Select(x => x.AfterCloseMessageAr)
            .FirstOrDefaultAsync();

        return string.IsNullOrWhiteSpace(message) ? _localizer["WindowClosedMessage"] : message;
    }

    private async Task<bool> CanViewNationalIdFullAsync(UserScope? scope)
    {
        if (User.IsInRole("SuperAdmin"))
        {
            return true;
        }

        return scope?.CanViewNationalIdFull ?? false;
    }

    private string FormatNationalId(byte[] encrypted, bool canViewFull)
    {
        var nationalId = _encryptionService.Decrypt(encrypted);
        if (canViewFull)
        {
            return nationalId;
        }

        if (nationalId.Length <= 4)
        {
            return nationalId;
        }

        return new string('*', nationalId.Length - 4) + nationalId[^4..];
    }

    private async Task LoadLookupsAsync(UserScope? scope, EmployeeFormViewModel model)
    {
        var ministries = _context.Ministries.AsNoTracking();
        var directorates = _context.Directorates.AsNoTracking();
        var departments = _context.Departments.AsNoTracking();
        var sections = _context.Sections.AsNoTracking();
        var units = _context.Units.AsNoTracking();

        if (!User.IsInRole("SuperAdmin") && scope is not null)
        {
            ministries = ministries.Where(x => x.Id == scope.MinistryId);
            directorates = directorates.Where(x => x.Id == scope.DirectorateId);
            if (scope.DepartmentId.HasValue)
            {
                departments = departments.Where(x => x.Id == scope.DepartmentId);
            }

            if (scope.SectionId.HasValue)
            {
                sections = sections.Where(x => x.Id == scope.SectionId);
            }

            if (scope.UnitId.HasValue)
            {
                units = units.Where(x => x.Id == scope.UnitId);
            }
        }

        ViewBag.MinistryId = new SelectList(await ministries.OrderBy(x => x.NameAr).ToListAsync(), "Id", "NameAr", model.MinistryId);
        ViewBag.DirectorateId = new SelectList(await directorates.OrderBy(x => x.NameAr).ToListAsync(), "Id", "NameAr", model.DirectorateId);
        ViewBag.DepartmentId = new SelectList(await departments.OrderBy(x => x.NameAr).ToListAsync(), "Id", "NameAr", model.DepartmentId);
        ViewBag.SectionId = new SelectList(await sections.OrderBy(x => x.NameAr).ToListAsync(), "Id", "NameAr", model.SectionId);
        ViewBag.UnitId = new SelectList(await units.OrderBy(x => x.NameAr).ToListAsync(), "Id", "NameAr", model.UnitId);
    }

    private Task<bool> ValidateScopeSelectionAsync(UserScope? scope, EmployeeFormViewModel model)
    {
        if (User.IsInRole("SuperAdmin"))
        {
            return Task.FromResult(true);
        }

        if (scope is null)
        {
            return Task.FromResult(false);
        }

        var valid = model.MinistryId == scope.MinistryId
                    && model.DirectorateId == scope.DirectorateId;

        if (scope.DepartmentId.HasValue)
        {
            valid = valid && model.DepartmentId == scope.DepartmentId;
        }

        if (scope.SectionId.HasValue)
        {
            valid = valid && model.SectionId == scope.SectionId;
        }

        if (scope.UnitId.HasValue)
        {
            valid = valid && model.UnitId == scope.UnitId;
        }

        return Task.FromResult(valid);
    }

    private async Task<bool> ValidateHierarchyAsync(EmployeeFormViewModel model)
    {
        if (model.MinistryId is null || model.DirectorateId is null)
        {
            return false;
        }

        var directorate = await _context.Directorates.AsNoTracking().FirstOrDefaultAsync(x => x.Id == model.DirectorateId);
        if (directorate is null || directorate.MinistryId != model.MinistryId)
        {
            return false;
        }

        if (model.DepartmentId.HasValue)
        {
            var department = await _context.Departments.AsNoTracking().FirstOrDefaultAsync(x => x.Id == model.DepartmentId);
            if (department is null || department.DirectorateId != model.DirectorateId)
            {
                return false;
            }
        }

        if (model.SectionId.HasValue)
        {
            var section = await _context.Sections.AsNoTracking().FirstOrDefaultAsync(x => x.Id == model.SectionId);
            if (section is null || section.DepartmentId != model.DepartmentId)
            {
                return false;
            }
        }

        if (model.UnitId.HasValue)
        {
            var unit = await _context.Units.AsNoTracking().FirstOrDefaultAsync(x => x.Id == model.UnitId);
            if (unit is null || unit.SectionId != model.SectionId)
            {
                return false;
            }
        }

        return true;
    }
}
