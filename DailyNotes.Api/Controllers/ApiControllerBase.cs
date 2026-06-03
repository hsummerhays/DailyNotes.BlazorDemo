using DailyNotes.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

namespace DailyNotes.Api.Controllers;

[Authorize]
[ApiController]
public abstract class ApiControllerBase(UserProvisioningService provisioning) : ControllerBase
{
    protected Task<(int TenantId, string UserId)> GetUserContextAsync()
    {
        var userId = User.GetObjectId();
        if (string.IsNullOrEmpty(userId)) throw new UnauthorizedAccessException();
        return provisioning.GetOrCreateAsync(userId);
    }
}
