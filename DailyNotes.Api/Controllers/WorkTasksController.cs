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
public class WorkTasksController : ControllerBase
{
    private readonly NotesDbContext _context;

    public WorkTasksController(NotesDbContext context)
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
    public async Task<ActionResult<IEnumerable<WorkTask>>> Get()
    {
        try 
        {
            var (tenantId, userId) = await GetUserContext();
            return await _context.WorkTasks
                .Where(t => t.TenantId == tenantId && t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in WorkTasksController.Get: {ex.Message}");
            return StatusCode(500, ex.Message);
        }
    }


    [HttpPost]
    public async Task<WorkTask> Post(WorkTask task)
    {
        var (tenantId, userId) = await GetUserContext();
        task.TenantId = tenantId;
        task.UserId = userId;
        task.CreatedAt = DateTime.UtcNow;
        task.UpdatedAt = DateTime.UtcNow;

        _context.WorkTasks.Add(task);
        await _context.SaveChangesAsync();
        return task;
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
    {
        var (tenantId, userId) = await GetUserContext();
        var task = await _context.WorkTasks
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == tenantId && t.UserId == userId);

        if (task == null) return NotFound();

        task.Status = status;
        task.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(task);
    }
    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, WorkTask task)
    {
        var (tenantId, userId) = await GetUserContext();
        if (id != task.Id) return BadRequest();

        var existing = await _context.WorkTasks
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

        await _context.SaveChangesAsync();
        return Ok(existing);
    }
}
