using DailyNotes.Api.Data;
using DailyNotes.Api.Services;
using DailyNotes.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DailyNotes.Api.Controllers;

[Route("api/[controller]")]
public class WorkTasksController(NotesDbContext context, UserProvisioningService provisioning)
    : ApiControllerBase(provisioning)
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<WorkTask>>> Get()
    {
        var (tenantId, userId) = await GetUserContextAsync();
        return await context.WorkTasks
            .Where(t => t.TenantId == tenantId && t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<WorkTask>> Post(WorkTask task)
    {
        var (tenantId, userId) = await GetUserContextAsync();
        task.TenantId = tenantId;
        task.UserId = userId;
        task.CreatedAt = DateTime.UtcNow;
        task.UpdatedAt = DateTime.UtcNow;

        context.WorkTasks.Add(task);
        await context.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { }, task);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, WorkTask task)
    {
        var (tenantId, userId) = await GetUserContextAsync();
        if (id != task.Id) return BadRequest();

        var existing = await context.WorkTasks
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == tenantId && t.UserId == userId);

        if (existing == null) return NotFound();

        existing.Name = task.Name;
        existing.Status = task.Status;
        existing.Comments = task.Comments;
        existing.StartDate = task.StartDate;
        existing.DueDate = task.DueDate;
        existing.ProjectId = task.ProjectId;
        existing.IsPinned = task.IsPinned;
        existing.Visibility = task.Visibility;
        existing.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return Ok(existing);
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
    {
        var (tenantId, userId) = await GetUserContextAsync();
        var task = await context.WorkTasks
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == tenantId && t.UserId == userId);

        if (task == null) return NotFound();

        task.Status = status;
        task.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
        return Ok(task);
    }
}
