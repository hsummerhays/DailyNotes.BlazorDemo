using DailyNotes.Api.Data;
using DailyNotes.Shared;
using DailyNotes.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace DailyNotes.Api.Services;

public class UserProvisioningService(NotesDbContext db)
{
    private const int DefaultTenantId = 1;

    public async Task<(int TenantId, string UserId)> GetOrCreateAsync(string userId)
    {
        var tenantUser = await db.TenantUsers.FirstOrDefaultAsync(tu => tu.UserId == userId);
        if (tenantUser != null) return (tenantUser.TenantId, userId);

        tenantUser = new TenantUser
        {
            TenantId = DefaultTenantId,
            UserId = userId,
            Role = DomainConstants.Role.Admin,
            CreatedAt = DateTime.UtcNow
        };
        db.TenantUsers.Add(tenantUser);
        await db.SaveChangesAsync();
        return (tenantUser.TenantId, userId);
    }
}
