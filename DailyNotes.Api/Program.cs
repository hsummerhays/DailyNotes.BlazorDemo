using DailyNotes.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using DailyNotes.Shared.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var azureAdConfig = builder.Configuration.GetSection("AzureAd");

// Safely retrieve the ClientId to avoid null dereference warnings
string? clientId = azureAdConfig["ClientId"];

if (builder.Environment.IsDevelopment() && (string.IsNullOrEmpty(clientId) || clientId.Contains('[')))
{
    builder.Services.AddAuthentication("Demo")
        .AddScheme<AuthenticationSchemeOptions, MockAuthHandler>("Demo", null);
}
else
{
    // Ensure the section exists before passing it to the Microsoft Identity helper
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApi(azureAdConfig);
}

builder.Services.AddAuthorization();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=notes.db";
builder.Services.AddDbContext<NotesDbContext>(options =>
{
    if (connectionString.Contains("Server="))
    {
        options.UseSqlServer(connectionString);
    }
    else
    {
        options.UseSqlite(connectionString);
    }
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddHealthChecks();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<NotesDbContext>();

    // Ensure the database directory exists (useful for Azure App Service persistent storage)
    var conn = db.Database.GetDbConnection();
    var dataSource = conn.DataSource;
    if (!string.IsNullOrEmpty(dataSource) && dataSource != ":memory:")
    {
        var directory = Path.GetDirectoryName(dataSource);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
            Console.WriteLine($"Created database directory: {directory}");
        }
    }

    // Ensure the database is created
    db.Database.EnsureCreated();
    Console.WriteLine("Database ensured created.");

    // Optional: Seed for a demo user if needed at startup
    var targetUserId = "072fbde7-eae8-4aee-b373-8ac17e74aba1";
    await SampleDataSeeder.SeedForUser(db, targetUserId);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");
app.MapGet("/", () => "DailyNotes API is running!");

try
{
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"CRITICAL ERROR DURING RUN: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
    throw;
}
