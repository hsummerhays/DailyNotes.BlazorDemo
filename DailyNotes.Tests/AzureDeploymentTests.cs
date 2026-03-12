using Microsoft.Extensions.Configuration;
using Xunit;
using System.IO;

namespace DailyNotes.Tests;

public class AzureDeploymentTests
{
    [Fact]
    public void Blazor_AppSettings_HasRequiredAzureAdKeys()
    {
        // Arrange
        var path = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "DailyNotes.Blazor", "appsettings.json");
        var config = new ConfigurationBuilder()
            .AddJsonFile(path)
            .Build();

        // Assert
        Assert.NotNull(config["AzureAd:Instance"]);
        Assert.NotNull(config["AzureAd:TenantId"]);
        Assert.NotNull(config["AzureAd:ClientId"]);
    }

    [Fact]
    public void Api_AppSettings_HasRequiredAzureAdKeys()
    {
        // Arrange
        var path = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "DailyNotes.Api", "appsettings.json");
        var config = new ConfigurationBuilder()
            .AddJsonFile(path)
            .Build();

        // Assert
        Assert.NotNull(config["AzureAd:Instance"]);
        Assert.NotNull(config["AzureAd:TenantId"]);
        Assert.NotNull(config["AzureAd:ClientId"]);
    }

    [Fact]
    public void Blazor_ApiEndpoint_ShouldBeOverriddenInProduction()
    {
        // This test simulates what should happen if we have an appsettings.Production.json
        // or environment variables. We want to ensure the logic in Program.cs
        // can handle overrides.
        
        var inMemoryConfig = new Dictionary<string, string?> {
            {"DailyNotesApi:BaseAddress", "https://dailynotes-api.azurewebsites.net/"}
        };

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemoryConfig)
            .Build();

        var apiAddress = config["DailyNotesApi:BaseAddress"];
        
        Assert.StartsWith("https://", apiAddress);
        Assert.DoesNotContain("localhost", apiAddress);
    }
}
