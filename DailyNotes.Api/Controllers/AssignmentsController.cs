using DailyNotes.Api.Data;
using DailyNotes.Api.Services;
using DailyNotes.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DailyNotes.Api.Controllers;

[Route("api/[controller]")]
public class AssignmentsController(NotesDbContext context, UserProvisioningService provisioning)
    : ApiControllerBase(provisioning)
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Assignment>>> Get()
    {
        var (tenantId, userId) = await GetUserContextAsync();
        return await context.Assignments
            .Where(a => a.TenantId == tenantId && a.UserId == userId)
            .OrderByDescending(a => a.DueDate)
            .ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<Assignment>> Post(Assignment assignment)
    {
        var (tenantId, userId) = await GetUserContextAsync();
        assignment.TenantId = tenantId;
        assignment.UserId = userId;
        assignment.CreatedAt = DateTime.UtcNow;
        assignment.UpdatedAt = DateTime.UtcNow;

        context.Assignments.Add(assignment);
        await context.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { }, assignment);
    }
}
