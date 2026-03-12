using Microsoft.AspNetCore.Mvc;
using DailyNotes.Api.Data;
using DailyNotes.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;

namespace DailyNotes.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class WorkDaysController : ControllerBase
{
    private readonly NotesDbContext _context;

    public WorkDaysController(NotesDbContext context)
    {
        _context = context;
    }

    private async Task<(int TenantId, string UserId)> GetUserContext()
    {
        var userId = User.GetObjectId();
        if (string.IsNullOrEmpty(userId)) throw new UnauthorizedAccessException();
        return await SampleDataSeeder.SeedForUser(_context, userId);
    }

    [HttpGet]
    public async Task<IEnumerable<WorkDay>> Get()
    {
        var (tenantId, userId) = await GetUserContext();
        return await _context.WorkDays
            .Where(w => w.TenantId == tenantId && w.UserId == userId)
            .OrderByDescending(w => w.WorkDate)
            .ToListAsync();
    }

    [HttpPost]
    public async Task<WorkDay> Post(WorkDay workDay)
    {
        var (tenantId, userId) = await GetUserContext();
        workDay.TenantId = tenantId;
        workDay.UserId = userId;
        workDay.CreatedAt = DateTime.UtcNow;
        workDay.UpdatedAt = DateTime.UtcNow;

        _context.WorkDays.Add(workDay);
        await _context.SaveChangesAsync();
        return workDay;
    }

    [HttpPost("clock-in")]
    public async Task<WorkDay> ClockIn()
    {
        var (tenantId, userId) = await GetUserContext();
        var today = DateTime.Today;

        var workDay = await _context.WorkDays
            .FirstOrDefaultAsync(w => w.WorkDate == today && w.TenantId == tenantId && w.UserId == userId);

        if (workDay == null)
        {
            workDay = new WorkDay
            {
                TenantId = tenantId,
                UserId = userId,
                WorkDate = today,
                TimeIn1 = DateTime.Now,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.WorkDays.Add(workDay);
        }
        else if (workDay.TimeIn1 == null)
        {
            workDay.TimeIn1 = DateTime.Now;
            workDay.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return workDay;
    }
}
