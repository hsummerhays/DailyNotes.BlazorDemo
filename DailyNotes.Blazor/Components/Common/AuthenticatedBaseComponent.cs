using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Identity.Web;
using DailyNotes.Blazor.Services;

namespace DailyNotes.Blazor.Components.Common;

public abstract class AuthenticatedBaseComponent : ComponentBase
{
    [Inject] protected HttpClient Http { get; set; } = default!;
    [Inject] protected IServiceProvider ServiceProvider { get; set; } = default!;
    protected MicrosoftIdentityConsentAndConditionalAccessHandler? ConsentHandler => 
        (MicrosoftIdentityConsentAndConditionalAccessHandler?)ServiceProvider.GetService(typeof(MicrosoftIdentityConsentAndConditionalAccessHandler));
    [Inject] protected NavigationManager Navigation { get; set; } = default!;
    [Inject] protected AuthenticationStateProvider AuthStateProvider { get; set; } = default!;
    [Inject] protected BlazorUserContext UserContext { get; set; } = default!;
    [Inject] protected IHttpContextAccessor HttpContextAccessor { get; set; } = default!;

    protected bool IsLoading { get; set; } = true;
    protected string? ErrorMessage { get; set; }

    /// <summary>
    /// Populates BlazorUserContext with both:
    ///  - The ClaimsPrincipal (for MSAL fallback)
    ///  - The raw access_token from OIDC SaveTokens (for reliable use in the SignalR circuit)
    /// </summary>
    private async Task EnsureUserContextAsync()
    {
        if (UserContext.User == null)
        {
            var state = await AuthStateProvider.GetAuthenticationStateAsync();
            UserContext.User = state.User;
        }

        // During the SSR pre-render phase, HttpContext is available and has the
        // access_token stored by SaveTokens. We grab it here and cache it on the
        // scoped BlazorUserContext so the DelegatingHandler can use it in the
        // interactive SignalR circuit where HttpContext is null.
        if (string.IsNullOrEmpty(UserContext.ApiAccessToken))
        {
            var httpContext = HttpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                var token = await httpContext.GetTokenAsync("access_token");
                if (!string.IsNullOrEmpty(token))
                {
                    UserContext.ApiAccessToken = token;
                }
            }
        }
    }

    protected async Task ExecuteApiCallAsync(Func<Task> action, bool showLoader = true)
    {
        if (showLoader) IsLoading = true;
        ErrorMessage = null;
        try
        {
            await EnsureUserContextAsync();
            await action();
        }
        catch (Exception ex)
        {
            var challengeEx = FindChallengeException(ex);
            if (challengeEx != null)
            {
                try
                {
                    if (ConsentHandler != null)
                    {
                        ConsentHandler.HandleException(challengeEx);
                    }
                    else
                    {
                        ErrorMessage = $"Consent required: {challengeEx.Message}";
                    }
                }
                catch (Exception)
                {
                    ErrorMessage = "Your session needs to be refreshed. Please sign out and sign in again.";
                }
                return;
            }

            ErrorMessage = $"Exception: {ex.Message}";
        }
        finally
        {
            if (showLoader) IsLoading = false;
            StateHasChanged();
        }
    }

    private Exception? FindChallengeException(Exception ex)
    {
        if (ex is MicrosoftIdentityWebChallengeUserException || ex is Microsoft.Identity.Client.MsalUiRequiredException) return ex;
        if (ex.InnerException != null) return FindChallengeException(ex.InnerException);
        if (ex is AggregateException ae)
        {
            foreach (var inner in ae.InnerExceptions)
            {
                var found = FindChallengeException(inner);
                if (found != null) return found;
            }
        }
        return null;
    }

    protected async Task<T?> GetFromJsonAsync<T>(string requestUri)
    {
        T? result = default;
        await ExecuteApiCallAsync(async () =>
        {
            result = await Http.GetFromJsonAsync<T>(requestUri);
        });
        return result;
    }
}
