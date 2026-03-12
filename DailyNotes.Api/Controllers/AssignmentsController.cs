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
public class AssignmentsController : ControllerBase
{
    private readonly NotesDbContext _context;

    public AssignmentsController(NotesDbContext context)
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
    public async Task<ActionResult<IEnumerable<Assignment>>> Get()
    {
        var (tenantId, userId) = await GetUserContext();
        return await _context.Assignments
            .Where(a => a.TenantId == tenantId && a.UserId == userId)
            .OrderByDescending(a => a.DueDate)
            .ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<Assignment>> Post(Assignment assignment)
    {
        var (tenantId, userId) = await GetUserContext();
        assignment.TenantId = tenantId;
        assignment.UserId = userId;
        assignment.CreatedAt = DateTime.UtcNow;
        assignment.UpdatedAt = DateTime.UtcNow;

        _context.Assignments.Add(assignment);
        await _context.SaveChangesAsync();
        return assignment;
    }
}
