using Microsoft.AspNetCore.Components;

namespace DailyNotes.Blazor.Services;

/// <summary>
/// Service that bridges the gap between the static SSR phase (where HttpContext exists)
/// and the interactive SignalR phase (where HttpContext is null), passing the 
/// access token securely using PersistentComponentState.
/// </summary>
public class TokenProvider : IDisposable
{
    private readonly PersistentComponentState _state;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private PersistingComponentStateSubscription _subscription;

    public string? AccessToken { get; private set; }

    public TokenProvider(PersistentComponentState state, IHttpContextAccessor httpContextAccessor)
    {
        _state = state;
        _httpContextAccessor = httpContextAccessor;

        if (_state.TryTakeFromJson<string>("api_token", out var token))
        {
            AccessToken = token;
        }

        _subscription = _state.RegisterOnPersisting(OnPersistingAsync);
    }

    private async Task OnPersistingAsync()
    {
        if (_httpContextAccessor.HttpContext != null)
        {
            var token = await Microsoft.AspNetCore.Authentication.AuthenticationHttpContextExtensions.GetTokenAsync(
                _httpContextAccessor.HttpContext, "access_token");
            if (!string.IsNullOrEmpty(token))
            {
                _state.PersistAsJson("api_token", token);
            }
        }
    }

    public void Dispose()
    {
        _subscription.Dispose();
    }
}
