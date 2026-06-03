using System.Security.Claims;

namespace DailyNotes.Blazor.Services;

/// <summary>
/// Scoped service that holds the current user's access token for the downstream API.
/// Populated during the initial SSR request (where HttpContext is available) and
/// carried across to the Blazor Server interactive circuit via PersistentComponentState.
/// This bypasses MSAL's distributed token cache which fails in SignalR circuits.
/// </summary>
public class BlazorUserContext
{
    public ClaimsPrincipal? User { get; set; }

    /// <summary>
    /// The raw access token for the downstream API, stored directly from OIDC / HttpContext.
    /// </summary>
    public string? ApiAccessToken { get; set; }
}
