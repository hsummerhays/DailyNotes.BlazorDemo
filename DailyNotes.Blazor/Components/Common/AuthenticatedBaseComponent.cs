using Microsoft.AspNetCore.Components;
using Microsoft.Identity.Web;
using System.Net.Http.Json;

namespace DailyNotes.Blazor.Components.Common;

public abstract class AuthenticatedBaseComponent : ComponentBase
{
    [Inject] protected HttpClient Http { get; set; } = default!;
    [Inject] protected MicrosoftIdentityConsentAndConditionalAccessHandler ConsentHandler { get; set; } = default!;
    [Inject] protected NavigationManager Navigation { get; set; } = default!;

    protected bool IsLoading { get; set; } = true;
    protected string? ErrorMessage { get; set; }

    protected async Task ExecuteApiCallAsync(Func<Task> action, bool showLoader = true)
    {
        if (showLoader) IsLoading = true;
        ErrorMessage = null;
        try
        {
            await action();
        }
        catch (Exception ex)
        {
            // Unwrap HttpRequestException / AggregateException to find the challenge
            var challengeEx = FindChallengeException(ex);
            if (challengeEx != null)
            {
                ConsentHandler.HandleException(challengeEx);
                // If it didn't throw a NavigationException (e.g. if the setting is disabled)
                // we should still stop here and not show the error message.
                return;
            }

            // If it didn't throw/redirect, it's a normal error
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
