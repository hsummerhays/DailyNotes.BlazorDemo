using DailyNotes.Api.Data;
using DailyNotes.Api.Services;
using DailyNotes.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DailyNotes.Api.Controllers;

[Route("api/[controller]")]
public class TagsController(NotesDbContext context, UserProvisioningService provisioning)
    : ApiControllerBase(provisioning)
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Tag>>> Get()
    {
        var (tenantId, _) = await GetUserContextAsync();
        return await context.Tags
            .Where(t => t.TenantId == tenantId)
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<Tag>> Post(Tag tag)
    {
        var (tenantId, _) = await GetUserContextAsync();
        tag.TenantId = tenantId;

        context.Tags.Add(tag);
        await context.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { }, tag);
    }
}
