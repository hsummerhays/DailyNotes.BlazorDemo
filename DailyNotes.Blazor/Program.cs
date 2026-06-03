using DailyNotes.Blazor.Components;
using DailyNotes.Blazor.Services;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var azureAdConfig = builder.Configuration.GetSection("AzureAd");
if (builder.Environment.IsDevelopment() && (string.IsNullOrEmpty(azureAdConfig["ClientId"]) || azureAdConfig["ClientId"]?.Contains("[") == true))
{
    builder.Services.AddAuthentication("Demo")
        .AddScheme<AuthenticationSchemeOptions, MockAuthHandler>("Demo", null);
}
else
{
    builder.Services.Configure<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.Cookie.Name = ".DailyNotes.Auth";
        options.Cookie.SameSite = SameSiteMode.None;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    });

    builder.Services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
    {
        options.CorrelationCookie.SameSite = SameSiteMode.None;
        options.NonceCookie.SameSite = SameSiteMode.None;
        options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
        options.NonceCookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Events.OnRedirectToIdentityProvider = context =>
        {
            context.ProtocolMessage.RedirectUri = context.ProtocolMessage.RedirectUri.Replace("http://", "https://");
            return Task.CompletedTask;
        };
    });

    var redisConnection = builder.Configuration.GetConnectionString("RedisCache");
    if (!string.IsNullOrEmpty(redisConnection))
    {
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnection;
            options.InstanceName = "DailyNotes_";
        });
    }
    else
    {
        builder.Services.AddDistributedMemoryCache();
    }
    builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApp(options =>
        {
            builder.Configuration.GetSection("AzureAd").Bind(options);
            options.ClientSecret = builder.Configuration["AzureAd:ClientSecret"];

            // SaveTokens stores the access_token in the authentication session
            // so we can retrieve it via HttpContext.GetTokenAsync in SSR phase
            options.SaveTokens = true;

            options.Events.OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"Authentication Failed: {context.Exception.Message}");
                return Task.CompletedTask;
            };
        })
        .EnableTokenAcquisitionToCallDownstreamApi(new string[] { $"api://{azureAdConfig["ClientId"]}/access_as_user" })
        .AddDistributedTokenCaches();
    builder.Services.AddMicrosoftIdentityConsentHandler();
}


builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews()
    .AddMicrosoftIdentityUI();

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHealthChecks();

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddHeaderPropagation(options => options.Headers.Add("Authorization"));

// Register the Blazor Server token bridge services
builder.Services.AddScoped<BlazorUserContext>();
builder.Services.AddScoped<TokenProvider>();
builder.Services.AddScoped<ApiAuthorizationMessageHandler>();


builder.Services.AddHttpClient("DailyNotesApi", client =>
{
    var apiBaseAddress = builder.Configuration["DailyNotesApi:BaseAddress"] ?? builder.Configuration["DailyNotesApi__BaseAddress"] ?? "http://localhost:5251/";
    Console.WriteLine($"[DEBUG] Blazor calling API at: {apiBaseAddress}");
    client.BaseAddress = new Uri(apiBaseAddress);
})
.AddHttpMessageHandler<ApiAuthorizationMessageHandler>();

// Provide simpler injection for components
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("DailyNotesApi"));

builder.Services.AddScoped<ThemeService>();

var app = builder.Build();

var forwardOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
forwardOptions.KnownIPNetworks.Clear();
forwardOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardOptions);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

    app.UseCookiePolicy(new CookiePolicyOptions
    {
        MinimumSameSitePolicy = builder.Environment.IsDevelopment() ? SameSiteMode.Unspecified : SameSiteMode.None,
        Secure = builder.Environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always
    });

app.UseAntiforgery();

app.UseHeaderPropagation();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapHealthChecks("/health");

app.MapControllers();
app.MapRazorPages();

app.Run();
