using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Rasad.Domain.Entities;
using Rasad.Infrastructure.Identity;
using Rasad.Infrastructure.Security;
using Rasad.Web.Models.Reports;
using Rasad.Web.Resources;

namespace Rasad.Web.Controllers;

[Authorize(Roles = "SuperAdmin")]
public class ReportsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IEncryptionService _encryptionService;
    private readonly IStringLocalizer<SharedResources> _localizer;

    public ReportsController(ApplicationDbContext context, IEncryptionService encryptionService, IStringLocalizer<SharedResources> localizer)
    {
        _context = context;
        _encryptionService = encryptionService;
        _localizer = localizer;
    }

    public async Task<IActionResult> Unverified(ReportFilterViewModel filter)
    {
        var query = ApplyFilter(_context.Employees
            .Include(x => x.Ministry)
            .Include(x => x.Directorate)
            .Include(x => x.Department)
            .Include(x => x.Section)
            .Include(x => x.Unit)
            .AsNoTracking()
            .Where(x => !x.IsDeleted && !x.IsVerified), filter);

        var employees = await query.OrderBy(x => x.FullName).ToListAsync();
        var list = employees.Select(e => new UnverifiedReportItemViewModel
        {
            Id = e.Id,
            FullName = e.FullName,
            JobNumber = e.JobNumber,
            NationalIdDisplay = FormatNationalId(e.NationalIdEncrypted, true),
            MinistryName = e.Ministry?.NameAr ?? string.Empty,
            DirectorateName = e.Directorate?.NameAr ?? string.Empty,
            DepartmentName = e.Department?.NameAr,
            SectionName = e.Section?.NameAr,
            UnitName = e.Unit?.NameAr
        }).ToList();

        await LoadLookupsAsync(filter);
        return View(list);
    }

    public async Task<IActionResult> ExportUnverified(ReportFilterViewModel filter)
    {
        var query = ApplyFilter(_context.Employees
            .Include(x => x.Ministry)
            .Include(x => x.Directorate)
            .Include(x => x.Department)
            .Include(x => x.Section)
            .Include(x => x.Unit)
            .AsNoTracking()
            .Where(x => !x.IsDeleted && !x.IsVerified), filter);

        var employees = await query.OrderBy(x => x.FullName).ToListAsync();

        using var workbook = new XLWorkbook();
        var sheet = workbook.AddWorksheet(_localizer["UnverifiedReportTitle"].Value);
        sheet.Cell(1, 1).Value = _localizer["EmployeeFullNameLabel"].Value;
        sheet.Cell(1, 2).Value = _localizer["JobNumberLabel"].Value;
        sheet.Cell(1, 3).Value = _localizer["NationalIdLabel"].Value;
        sheet.Cell(1, 4).Value = _localizer["MinistryLabel"].Value;
        sheet.Cell(1, 5).Value = _localizer["DirectorateLabel"].Value;
        sheet.Cell(1, 6).Value = _localizer["DepartmentLabel"].Value;
        sheet.Cell(1, 7).Value = _localizer["SectionLabel"].Value;
        sheet.Cell(1, 8).Value = _localizer["UnitLabel"].Value;

        var row = 2;
        foreach (var e in employees)
        {
            sheet.Cell(row, 1).Value = e.FullName;
            sheet.Cell(row, 2).Value = e.JobNumber;
            sheet.Cell(row, 3).Value = FormatNationalId(e.NationalIdEncrypted, true);
            sheet.Cell(row, 4).Value = e.Ministry?.NameAr;
            sheet.Cell(row, 5).Value = e.Directorate?.NameAr;
            sheet.Cell(row, 6).Value = e.Department?.NameAr;
            sheet.Cell(row, 7).Value = e.Section?.NameAr;
            sheet.Cell(row, 8).Value = e.Unit?.NameAr;
            row++;
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "غير-المتحققين.xlsx");
    }

    public async Task<IActionResult> Ghost()
    {
        var groups = await _context.Employees.AsNoTracking()
            .Where(x => !x.IsDeleted)
            .GroupBy(x => x.NationalIdHash)
            .Select(g => new
            {
                Hash = g.Key,
                Names = g.Select(x => x.NormalizedName).Distinct().ToList()
            })
            .Where(x => x.Names.Count > 1)
            .ToListAsync();

        var list = groups.Select(g => new GhostEmployeeGroupViewModel
        {
            NationalIdDisplay = GetMaskedNationalId(g.Hash),
            NamesCount = g.Names.Count,
            Names = g.Names
        }).ToList();

        return View(list);
    }

    public async Task<IActionResult> ExportGhost()
    {
        var groups = await _context.Employees.AsNoTracking()
            .Where(x => !x.IsDeleted)
            .GroupBy(x => x.NationalIdHash)
            .Select(g => new
            {
                Hash = g.Key,
                Names = g.Select(x => x.NormalizedName).Distinct().ToList()
            })
            .Where(x => x.Names.Count > 1)
            .ToListAsync();

        using var workbook = new XLWorkbook();
        var sheet = workbook.AddWorksheet(_localizer["GhostReportTitle"].Value);
        sheet.Cell(1, 1).Value = _localizer["NationalIdLabel"].Value;
        sheet.Cell(1, 2).Value = _localizer["NamesCountLabel"].Value;
        sheet.Cell(1, 3).Value = _localizer["NamesLabel"].Value;

        var row = 2;
        foreach (var group in groups)
        {
            sheet.Cell(row, 1).Value = GetMaskedNationalId(group.Hash);
            sheet.Cell(row, 2).Value = group.Names.Count;
            sheet.Cell(row, 3).Value = string.Join(" | ", group.Names);
            row++;
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "الموظفون-الوهميون.xlsx");
    }

    public async Task<IActionResult> Duplicate()
    {
        var groups = await _context.Employees.AsNoTracking()
            .Include(x => x.Ministry)
            .Where(x => !x.IsDeleted)
            .GroupBy(x => new { x.NationalIdHash, x.NormalizedName })
            .Select(g => new
            {
                g.Key.NationalIdHash,
                g.Key.NormalizedName,
                Ministries = g.Select(x => x.Ministry!.NameAr).Distinct().ToList()
            })
            .Where(x => x.Ministries.Count > 1)
            .ToListAsync();

        var list = groups.Select(g => new DuplicateEmployeeGroupViewModel
        {
            NationalIdDisplay = GetMaskedNationalId(g.NationalIdHash),
            NormalizedName = g.NormalizedName,
            Ministries = g.Ministries
        }).ToList();

        return View(list);
    }

    public async Task<IActionResult> ExportDuplicate()
    {
        var groups = await _context.Employees.AsNoTracking()
            .Include(x => x.Ministry)
            .Where(x => !x.IsDeleted)
            .GroupBy(x => new { x.NationalIdHash, x.NormalizedName })
            .Select(g => new
            {
                g.Key.NationalIdHash,
                g.Key.NormalizedName,
                Ministries = g.Select(x => x.Ministry!.NameAr).Distinct().ToList()
            })
            .Where(x => x.Ministries.Count > 1)
            .ToListAsync();

        using var workbook = new XLWorkbook();
        var sheet = workbook.AddWorksheet(_localizer["DuplicateReportTitle"].Value);
        sheet.Cell(1, 1).Value = _localizer["NationalIdLabel"].Value;
        sheet.Cell(1, 2).Value = _localizer["EmployeeFullNameLabel"].Value;
        sheet.Cell(1, 3).Value = _localizer["MinistriesLabel"].Value;

        var row = 2;
        foreach (var group in groups)
        {
            sheet.Cell(row, 1).Value = GetMaskedNationalId(group.NationalIdHash);
            sheet.Cell(row, 2).Value = group.NormalizedName;
            sheet.Cell(row, 3).Value = string.Join(" | ", group.Ministries);
            row++;
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "الازدواج-الوظيفي.xlsx");
    }

    private static IQueryable<Employee> ApplyFilter(IQueryable<Employee> query, ReportFilterViewModel filter)
    {
        if (filter.MinistryId.HasValue)
        {
            query = query.Where(x => x.MinistryId == filter.MinistryId.Value);
        }

        if (filter.DirectorateId.HasValue)
        {
            query = query.Where(x => x.DirectorateId == filter.DirectorateId.Value);
        }

        if (filter.DepartmentId.HasValue)
        {
            query = query.Where(x => x.DepartmentId == filter.DepartmentId.Value);
        }

        if (filter.SectionId.HasValue)
        {
            query = query.Where(x => x.SectionId == filter.SectionId.Value);
        }

        if (filter.UnitId.HasValue)
        {
            query = query.Where(x => x.UnitId == filter.UnitId.Value);
        }

        return query;
    }

    private async Task LoadLookupsAsync(ReportFilterViewModel filter)
    {
        ViewBag.MinistryId = new SelectList(await _context.Ministries.AsNoTracking().OrderBy(x => x.NameAr).ToListAsync(), "Id", "NameAr", filter.MinistryId);
        ViewBag.DirectorateId = new SelectList(await _context.Directorates.AsNoTracking().OrderBy(x => x.NameAr).ToListAsync(), "Id", "NameAr", filter.DirectorateId);
        ViewBag.DepartmentId = new SelectList(await _context.Departments.AsNoTracking().OrderBy(x => x.NameAr).ToListAsync(), "Id", "NameAr", filter.DepartmentId);
        ViewBag.SectionId = new SelectList(await _context.Sections.AsNoTracking().OrderBy(x => x.NameAr).ToListAsync(), "Id", "NameAr", filter.SectionId);
        ViewBag.UnitId = new SelectList(await _context.Units.AsNoTracking().OrderBy(x => x.NameAr).ToListAsync(), "Id", "NameAr", filter.UnitId);
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

    private string GetMaskedNationalId(byte[] hash)
    {
        var employee = _context.Employees.AsNoTracking().FirstOrDefault(x => x.NationalIdHash == hash && !x.IsDeleted);
        if (employee is null)
        {
            return string.Empty;
        }

        return FormatNationalId(employee.NationalIdEncrypted, true);
    }
}
