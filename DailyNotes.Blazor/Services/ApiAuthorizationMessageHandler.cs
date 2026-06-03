using Microsoft.Identity.Web;
using System.Net.Http.Headers;

namespace DailyNotes.Blazor.Services;

/// <summary>
/// Attaches a Bearer token to outgoing API requests.
/// Strategy (in order):
///   1. Use the raw access token saved in BlazorUserContext.ApiAccessToken (set during SSR from SaveTokens).
///   2. Fall back to ITokenAcquisition with the explicit ClaimsPrincipal from BlazorUserContext.User.
/// This two-step approach handles both the SSR pre-render phase and the interactive SignalR circuit.
/// </summary>
public class ApiAuthorizationMessageHandler : DelegatingHandler
{
    private readonly ITokenAcquisition? _tokenAcquisition;
    private readonly BlazorUserContext _userContext;
    private readonly TokenProvider _tokenProvider;
    private readonly string _scope;

    public ApiAuthorizationMessageHandler(
        BlazorUserContext userContext,
        TokenProvider tokenProvider,
        IConfiguration configuration,
        IServiceProvider serviceProvider)
    {
        _tokenAcquisition = (ITokenAcquisition?)serviceProvider.GetService(typeof(ITokenAcquisition));
        _userContext = userContext;
        _tokenProvider = tokenProvider;
        var clientId = configuration["AzureAd__ClientId"] ?? configuration["AzureAd:ClientId"];
        _scope = $"api://{clientId}/access_as_user";
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        string? token = null;

        // Strategy 1: use the raw token saved during SSR, carried over by PersistentComponentState
        if (!string.IsNullOrEmpty(_tokenProvider.AccessToken))
        {
            token = _tokenProvider.AccessToken;
        }
        else if (!string.IsNullOrEmpty(_userContext.ApiAccessToken))
        {
            // Fallback 1.5: SSR context memory fallback
            token = _userContext.ApiAccessToken;
        }
        else
        {
            // Strategy 2: ask MSAL with an explicit user (works when MSAL cache is warm)
            if (_tokenAcquisition == null)
            {
                // In mock mode, we do not require a real token
                token = "mock-token";
            }
            else
            {
                try
                {
                    token = await _tokenAcquisition.GetAccessTokenForUserAsync(
                        new[] { _scope },
                        user: _userContext.User);
                }
                catch (MicrosoftIdentityWebChallengeUserException)
                {
                    throw;
                }
                catch (Microsoft.Identity.Client.MsalUiRequiredException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new MicrosoftIdentityWebChallengeUserException(
                        new Microsoft.Identity.Client.MsalUiRequiredException("user_null", ex.Message, ex),
                        new[] { _scope });
                }
            }
        }

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
