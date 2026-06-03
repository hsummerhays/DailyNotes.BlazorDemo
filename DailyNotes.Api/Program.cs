using DailyNotes.Api.Data;
using DailyNotes.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

var azureAdConfig = builder.Configuration.GetSection("AzureAd");
string? clientId = azureAdConfig["ClientId"];

if (builder.Environment.IsDevelopment() && (string.IsNullOrEmpty(clientId) || clientId.Contains('[')))
{
    builder.Services.AddAuthentication("Demo")
        .AddScheme<AuthenticationSchemeOptions, MockAuthHandler>("Demo", null);
}
else
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApi(azureAdConfig);
}

builder.Services.AddAuthorization();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=notes.db";
var dbProvider = builder.Configuration["DatabaseProvider"] ?? "Sqlite";

builder.Services.AddDbContext<NotesDbContext>(options =>
{
    if (dbProvider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
        options.UseSqlServer(connectionString);
    else
        options.UseSqlite(connectionString);
});

builder.Services.AddScoped<UserProvisioningService>();
builder.Services.AddScoped<SampleDataSeeder>();

builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddHealthChecks();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? Array.Empty<string>();
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseExceptionHandler(err => err.Run(async ctx =>
{
    ctx.Response.StatusCode = 500;
    ctx.Response.ContentType = "application/json";
    await ctx.Response.WriteAsJsonAsync(new { error = "An unexpected error occurred." });
}));

using (var scope = app.Services.CreateScope())
{
    var startupLogger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
    var db = scope.ServiceProvider.GetRequiredService<NotesDbContext>();

    var conn = db.Database.GetDbConnection();
    var dataSource = conn.DataSource;
    if (!string.IsNullOrEmpty(dataSource) && dataSource != ":memory:")
    {
        var directory = Path.GetDirectoryName(dataSource);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
            startupLogger.LogInformation("Created database directory: {Directory}", directory);
        }
    }

    db.Database.EnsureCreated();
    startupLogger.LogInformation("Database ensured created");

    var seedUserId = app.Configuration["SeedUserId"] ?? "demo-user-oid";
    var seeder = scope.ServiceProvider.GetRequiredService<SampleDataSeeder>();
    await seeder.SeedForUserAsync(seedUserId);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// HTTPS termination is handled upstream by Azure App Service / the load balancer.
// app.UseHttpsRedirection();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");
app.MapGet("/", () => "DailyNotes API is running!");

app.Run();
