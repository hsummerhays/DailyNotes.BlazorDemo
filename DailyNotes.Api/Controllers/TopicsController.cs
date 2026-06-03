using DailyNotes.Api.Data;
using DailyNotes.Api.Services;
using DailyNotes.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DailyNotes.Api.Controllers;

[Route("api/[controller]")]
public class TopicsController(NotesDbContext context, UserProvisioningService provisioning)
    : ApiControllerBase(provisioning)
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Topic>>> Get()
    {
        var (tenantId, userId) = await GetUserContextAsync();
        return await context.Topics
            .Where(t => t.TenantId == tenantId && t.UserId == userId)
            .OrderBy(t => t.Title)
            .ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<Topic>> Post(Topic topic)
    {
        var (tenantId, userId) = await GetUserContextAsync();
        topic.TenantId = tenantId;
        topic.UserId = userId;
        topic.CreatedAt = DateTime.UtcNow;
        topic.UpdatedAt = DateTime.UtcNow;

        context.Topics.Add(topic);
        await context.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { }, topic);
    }
}
