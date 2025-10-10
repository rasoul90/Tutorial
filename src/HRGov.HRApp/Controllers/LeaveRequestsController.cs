using HRGov.HRApp.Data;
using HRGov.HRApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HRGov.HRApp.Controllers;

[Authorize(Roles = "HRManager,Supervisor")]
public class LeaveRequestsController : Controller
{
    private readonly ApplicationDbContext _context;

    public LeaveRequestsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var leaves = await _context.LeaveRequests
            .Include(l => l.Employee)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();
        return View(leaves);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateEmployeesDropDownAsync();
        return View(new LeaveRequest
        {
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(1),
            Status = LeaveStatus.Pending
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LeaveRequest leaveRequest)
    {
        if (!ModelState.IsValid)
        {
            await PopulateEmployeesDropDownAsync();
            return View(leaveRequest);
        }

        _context.LeaveRequests.Add(leaveRequest);
        await _context.SaveChangesAsync();
        TempData["Success"] = "تم تسجيل الإجازة";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Review(int id)
    {
        var leaveRequest = await _context.LeaveRequests
            .Include(l => l.Employee)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (leaveRequest == null)
        {
            return NotFound();
        }

        await PopulateEmployeesDropDownAsync();
        return View(leaveRequest);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Review(int id, LeaveStatus status, int? approvedById)
    {
        var leaveRequest = await _context.LeaveRequests.FindAsync(id);
        if (leaveRequest == null)
        {
            return NotFound();
        }

        leaveRequest.Status = status;
        leaveRequest.ApprovedById = approvedById;
        leaveRequest.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        TempData["Success"] = "تم تحديث حالة الإجازة";
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateEmployeesDropDownAsync()
    {
        ViewBag.Employees = new SelectList(await _context.Employees
            .OrderBy(e => e.FullName)
            .ToListAsync(), "Id", "FullName");
    }
}
