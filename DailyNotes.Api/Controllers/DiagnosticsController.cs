using DailyNotes.Api.Data;
using DailyNotes.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DailyNotes.Api.Controllers;

[Route("api/[controller]")]
public class DiagnosticsController(NotesDbContext context, UserProvisioningService provisioning)
    : ApiControllerBase(provisioning)
{
    [HttpGet("userlist")]
    public async Task<IActionResult> UserList()
    {
        var (tenantId, _) = await GetUserContextAsync();
        var users = await context.TenantUsers
            .Where(u => u.TenantId == tenantId)
            .ToListAsync();
        return Ok(users);
    }

    [HttpGet("dbstatus")]
    public async Task<IActionResult> DbStatus()
    {
        var projectCount = await context.Projects.CountAsync();
        var userCount = await context.TenantUsers.CountAsync();
        var taskCount = await context.WorkTasks.CountAsync();
        var noteCount = await context.Notes.CountAsync();

        return Ok(new
        {
            projects = projectCount,
            users = userCount,
            tasks = taskCount,
            notes = noteCount,
            dbType = context.Database.ProviderName
        });
    }

    [HttpGet("test")]
    public IActionResult Test() =>
        Ok(new { message = "Diagnostics endpoint is working!", time = DateTime.UtcNow });
}
