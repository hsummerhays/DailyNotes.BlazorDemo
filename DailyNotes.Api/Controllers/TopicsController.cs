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
public class TopicsController : ControllerBase
{
    private readonly NotesDbContext _context;

    public TopicsController(NotesDbContext context)
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
    public async Task<ActionResult<IEnumerable<Topic>>> Get()
    {
        var (tenantId, userId) = await GetUserContext();
        return await _context.Topics
            .Where(t => t.TenantId == tenantId && t.UserId == userId)
            .OrderBy(t => t.Title)
            .ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<Topic>> Post(Topic topic)
    {
        var (tenantId, userId) = await GetUserContext();
        topic.TenantId = tenantId;
        topic.UserId = userId;
        topic.CreatedAt = DateTime.UtcNow;
        topic.UpdatedAt = DateTime.UtcNow;

        _context.Topics.Add(topic);
        await _context.SaveChangesAsync();
        return topic;
    }
}
