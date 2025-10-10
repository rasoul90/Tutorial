using HRGov.HRApp.Data;
using HRGov.HRApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRGov.HRApp.Controllers;

[Authorize(Roles = "HRManager,TrainingCoordinator")]
public class TrainingSessionsController : Controller
{
    private readonly ApplicationDbContext _context;

    public TrainingSessionsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var sessions = await _context.TrainingSessions
            .Include(t => t.Participants)
                .ThenInclude(p => p.Employee)
            .OrderByDescending(t => t.StartDate)
            .ToListAsync();
        return View(sessions);
    }

    public IActionResult Create()
    {
        return View(new TrainingSession
        {
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(3)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TrainingSession session)
    {
        if (!ModelState.IsValid)
        {
            return View(session);
        }

        _context.TrainingSessions.Add(session);
        await _context.SaveChangesAsync();
        TempData["Success"] = "تم إنشاء دورة تدريبية";
        return RedirectToAction(nameof(Index));
    }
}
