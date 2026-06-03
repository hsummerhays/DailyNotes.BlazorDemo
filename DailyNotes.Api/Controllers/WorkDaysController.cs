using DailyNotes.Api.Data;
using DailyNotes.Api.Services;
using DailyNotes.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DailyNotes.Api.Controllers;

[Route("api/[controller]")]
public class WorkDaysController(NotesDbContext context, UserProvisioningService provisioning)
    : ApiControllerBase(provisioning)
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<WorkDay>>> Get()
    {
        var (tenantId, userId) = await GetUserContextAsync();
        return await context.WorkDays
            .Where(w => w.TenantId == tenantId && w.UserId == userId)
            .OrderByDescending(w => w.WorkDate)
            .ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<WorkDay>> Post(WorkDay workDay)
    {
        var (tenantId, userId) = await GetUserContextAsync();
        workDay.TenantId = tenantId;
        workDay.UserId = userId;
        workDay.CreatedAt = DateTime.UtcNow;
        workDay.UpdatedAt = DateTime.UtcNow;

        context.WorkDays.Add(workDay);
        await context.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { }, workDay);
    }

    [HttpPost("clock-in")]
    public async Task<ActionResult<WorkDay>> ClockIn()
    {
        var (tenantId, userId) = await GetUserContextAsync();
        var today = DateTime.Today;

        var workDay = await context.WorkDays
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
            context.WorkDays.Add(workDay);
        }
        else if (workDay.TimeIn1 == null)
        {
            workDay.TimeIn1 = DateTime.Now;
            workDay.UpdatedAt = DateTime.UtcNow;
        }

        await context.SaveChangesAsync();
        return Ok(workDay);
    }
}
