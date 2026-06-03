using DailyNotes.Api.Data;
using DailyNotes.Api.Services;
using DailyNotes.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DailyNotes.Api.Controllers;

[Route("api/[controller]")]
public class ProjectsController(NotesDbContext context, UserProvisioningService provisioning)
    : ApiControllerBase(provisioning)
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Project>>> Get()
    {
        var (tenantId, userId) = await GetUserContextAsync();
        return await context.Projects
            .Where(p => p.TenantId == tenantId && p.UserId == userId)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<Project>> Post(Project project)
    {
        var (tenantId, userId) = await GetUserContextAsync();
        project.TenantId = tenantId;
        project.UserId = userId;
        project.CreatedAt = DateTime.UtcNow;
        project.UpdatedAt = DateTime.UtcNow;

        context.Projects.Add(project);
        await context.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { }, project);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, Project project)
    {
        var (tenantId, userId) = await GetUserContextAsync();
        if (id != project.Id) return BadRequest();

        var existing = await context.Projects
            .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId && p.UserId == userId);

        if (existing == null) return NotFound();

        existing.Name = project.Name;
        existing.Category = project.Category;
        existing.Visibility = project.Visibility;
        existing.CreatedDate = project.CreatedDate;
        existing.CompletedDate = project.CompletedDate;
        existing.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return Ok(existing);
    }
}
