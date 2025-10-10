using HRGov.HRApp.Data;
using HRGov.HRApp.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace HRGov.HRApp.Services;

public class LookupService : ILookupService
{
    private readonly ApplicationDbContext _context;

    public LookupService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<LookupCollections> GetLookupsAsync()
    {
        var departmentsTask = _context.Departments
            .OrderBy(d => d.Name)
            .Select(d => new LookupItem(d.Id, d.Name))
            .ToListAsync();

        var jobTitlesTask = _context.JobTitles
            .OrderBy(j => j.Name)
            .Select(j => new LookupItem(j.Id, j.Name))
            .ToListAsync();

        await Task.WhenAll(departmentsTask, jobTitlesTask);

        return new LookupCollections
        {
            Departments = await departmentsTask,
            JobTitles = await jobTitlesTask
        };
    }
}
