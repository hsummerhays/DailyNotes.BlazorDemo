using DailyNotes.Api.Data;
using DailyNotes.Api.Services;
using DailyNotes.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DailyNotes.Api.Controllers;

[Route("api/[controller]")]
public class NotesController(NotesDbContext context, UserProvisioningService provisioning)
    : ApiControllerBase(provisioning)
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Note>>> Get([FromQuery] int? taskId = null)
    {
        var (tenantId, userId) = await GetUserContextAsync();
        var query = context.Notes.Where(n => n.TenantId == tenantId && n.UserId == userId);

        if (taskId.HasValue)
            query = query.Where(n => n.WorkTaskId == taskId.Value);

        return await query.OrderByDescending(n => n.NoteDate).ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<Note>> Post(Note note)
    {
        var (tenantId, userId) = await GetUserContextAsync();
        note.TenantId = tenantId;
        note.UserId = userId;
        note.CreatedAt = DateTime.UtcNow;
        note.UpdatedAt = DateTime.UtcNow;

        context.Notes.Add(note);
        await context.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { }, note);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, Note note)
    {
        var (tenantId, userId) = await GetUserContextAsync();
        if (id != note.Id) return BadRequest();

        var existing = await context.Notes
            .FirstOrDefaultAsync(n => n.Id == id && n.TenantId == tenantId && n.UserId == userId);

        if (existing == null) return NotFound();

        existing.Content = note.Content;
        existing.NoteDate = note.NoteDate;
        existing.WorkTaskId = note.WorkTaskId;
        existing.TimeMinutes = note.TimeMinutes;
        existing.IsPinned = note.IsPinned;
        existing.Visibility = note.Visibility;
        existing.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return Ok(existing);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var (tenantId, userId) = await GetUserContextAsync();
        var note = await context.Notes
            .FirstOrDefaultAsync(n => n.Id == id && n.TenantId == tenantId && n.UserId == userId);

        if (note == null) return NotFound();

        context.Notes.Remove(note);
        await context.SaveChangesAsync();
        return NoContent();
    }
}
