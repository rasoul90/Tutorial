using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Rasad.Domain.Entities;
using Rasad.Infrastructure.Identity;
using Rasad.Web.Models.Users;
using Rasad.Web.Resources;

namespace Rasad.Web.Controllers;

[Authorize(Roles = "MinistryAdmin")]
public class MinistryUsersController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IStringLocalizer<SharedResources> _localizer;

    public MinistryUsersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IStringLocalizer<SharedResources> localizer)
    {
        _context = context;
        _userManager = userManager;
        _localizer = localizer;
    }

    public async Task<IActionResult> Index()
    {
        var scope = await GetScopeAsync();
        if (scope is null)
        {
            return Forbid();
        }

        var users = await _context.UserScopes
            .Include(x => x.Ministry)
            .Include(x => x.Directorate)
            .Include(x => x.Department)
            .Include(x => x.Section)
            .Include(x => x.Unit)
            .Where(x => x.MinistryId == scope.MinistryId)
            .AsNoTracking()
            .ToListAsync();

        var userIds = users.Select(x => x.UserId).Distinct().ToList();
        var userInfo = await _userManager.Users
            .Where(x => userIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x);

        var list = users.Select(scopeItem => new MinistryUserListItemViewModel
        {
            UserId = scopeItem.UserId,
            UserName = userInfo.TryGetValue(scopeItem.UserId, out var user) ? user.UserName ?? string.Empty : string.Empty,
            Email = userInfo.TryGetValue(scopeItem.UserId, out user) ? user.Email ?? string.Empty : string.Empty,
            MinistryName = scopeItem.Ministry?.NameAr ?? string.Empty,
            DirectorateName = scopeItem.Directorate?.NameAr ?? string.Empty,
            DepartmentName = scopeItem.Department?.NameAr,
            SectionName = scopeItem.Section?.NameAr,
            UnitName = scopeItem.Unit?.NameAr,
            CanImportExcel = scopeItem.CanImportExcel,
            CanExportExcel = scopeItem.CanExportExcel,
            CanViewNationalIdFull = scopeItem.CanViewNationalIdFull
        }).ToList();

        return View(list);
    }

    public async Task<IActionResult> Create()
    {
        var scope = await GetScopeAsync();
        if (scope is null)
        {
            return Forbid();
        }

        var model = new MinistryUserCreateViewModel
        {
            MinistryId = scope.MinistryId
        };

        await LoadLookupsAsync(scope, model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MinistryUserCreateViewModel model)
    {
        var scope = await GetScopeAsync();
        if (scope is null)
        {
            return Forbid();
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

        var user = new ApplicationUser
        {
            UserName = model.UserName,
            Email = model.Email
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            await LoadLookupsAsync(scope, model);
            return View(model);
        }

        await _userManager.AddToRoleAsync(user, "MinistryUser");

        var userScope = new UserScope
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            MinistryId = model.MinistryId!.Value,
            DirectorateId = model.DirectorateId!.Value,
            DepartmentId = model.DepartmentId,
            SectionId = model.SectionId,
            UnitId = model.UnitId,
            CanImportExcel = model.CanImportExcel,
            CanExportExcel = model.CanExportExcel,
            CanViewNationalIdFull = model.CanViewNationalIdFull
        };

        _context.UserScopes.Add(userScope);
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

    private async Task LoadLookupsAsync(UserScope scope, MinistryUserCreateViewModel model)
    {
        var ministries = _context.Ministries.AsNoTracking().Where(x => x.Id == scope.MinistryId);
        var directorates = _context.Directorates.AsNoTracking().Where(x => x.MinistryId == scope.MinistryId);
        var departments = _context.Departments.AsNoTracking();
        var sections = _context.Sections.AsNoTracking();
        var units = _context.Units.AsNoTracking();

        if (scope.DepartmentId.HasValue)
        {
            departments = departments.Where(x => x.Id == scope.DepartmentId);
        }
        else
        {
            departments = departments.Where(x => x.DirectorateId == model.DirectorateId);
        }

        if (scope.SectionId.HasValue)
        {
            sections = sections.Where(x => x.Id == scope.SectionId);
        }
        else
        {
            sections = sections.Where(x => x.DepartmentId == model.DepartmentId);
        }

        if (scope.UnitId.HasValue)
        {
            units = units.Where(x => x.Id == scope.UnitId);
        }
        else
        {
            units = units.Where(x => x.SectionId == model.SectionId);
        }

        ViewBag.MinistryId = new SelectList(await ministries.OrderBy(x => x.NameAr).ToListAsync(), "Id", "NameAr", model.MinistryId);
        ViewBag.DirectorateId = new SelectList(await directorates.OrderBy(x => x.NameAr).ToListAsync(), "Id", "NameAr", model.DirectorateId);
        ViewBag.DepartmentId = new SelectList(await departments.OrderBy(x => x.NameAr).ToListAsync(), "Id", "NameAr", model.DepartmentId);
        ViewBag.SectionId = new SelectList(await sections.OrderBy(x => x.NameAr).ToListAsync(), "Id", "NameAr", model.SectionId);
        ViewBag.UnitId = new SelectList(await units.OrderBy(x => x.NameAr).ToListAsync(), "Id", "NameAr", model.UnitId);
    }

    private static Task<bool> ValidateScopeSelectionAsync(UserScope scope, MinistryUserCreateViewModel model)
    {
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

    private async Task<bool> ValidateHierarchyAsync(MinistryUserCreateViewModel model)
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
