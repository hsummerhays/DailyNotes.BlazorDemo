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
public class TagsController : ControllerBase
{
    private readonly NotesDbContext _context;

    public TagsController(NotesDbContext context)
    {
        _context = context;
    }

    private async Task<(int TenantId, string UserId)> GetUserContext()
    {
        var userId = User.GetObjectId();
        if (string.IsNullOrEmpty(userId)) throw new UnauthorizedAccessException();

        var tenantUser = await _context.TenantUsers
            .FirstOrDefaultAsync(tu => tu.UserId == userId);
        
        return (tenantUser?.TenantId ?? 1, userId);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Tag>>> Get()
    {
        var (tenantId, _) = await GetUserContext();
        return await _context.Tags
            .Where(t => t.TenantId == tenantId)
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<Tag>> Post(Tag tag)
    {
        var (tenantId, _) = await GetUserContext();
        tag.TenantId = tenantId;

        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();
        return tag;
    }
}
