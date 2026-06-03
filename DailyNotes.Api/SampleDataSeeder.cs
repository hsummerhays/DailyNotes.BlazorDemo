using DailyNotes.Api.Data;
using DailyNotes.Api.Services;
using DailyNotes.Shared;
using DailyNotes.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace DailyNotes.Api.Services;

public class SampleDataSeeder(NotesDbContext db, UserProvisioningService provisioning, ILogger<SampleDataSeeder> logger)
{
    public async Task SeedForUserAsync(string userId)
    {
        var (tenantId, _) = await provisioning.GetOrCreateAsync(userId);

        var projectCount = await db.Projects.CountAsync(p => p.UserId == userId);
        if (projectCount > 0) return;

        logger.LogInformation("Starting sample data seeding for user: {UserId}", userId);

        var productLaunch = new Project { TenantId = tenantId, UserId = userId, Name = "Product Launch 2024", Category = "Marketing", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var infraMigration = new Project { TenantId = tenantId, UserId = userId, Name = "Infrastructure Migration", Category = "IT", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        db.Projects.AddRange(productLaunch, infraMigration);
        await db.SaveChangesAsync();

        db.WorkDays.Add(new WorkDay
        {
            TenantId = tenantId,
            UserId = userId,
            WorkDate = DateTime.Today,
            TimeIn1 = DateTime.Today.AddHours(9),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        db.WorkTasks.AddRange(
            new WorkTask { TenantId = tenantId, UserId = userId, ProjectId = productLaunch.Id, Name = "Draft social media announcement", Status = DomainConstants.TaskStatus.Todo, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new WorkTask { TenantId = tenantId, UserId = userId, ProjectId = productLaunch.Id, Name = "Prepare slide deck for kickoff", Status = DomainConstants.TaskStatus.InProgress, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new WorkTask { TenantId = tenantId, UserId = userId, ProjectId = infraMigration.Id, Name = "Audit cloud resource usage", Status = DomainConstants.TaskStatus.Done, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        );

        db.Notes.AddRange(
            new Note { TenantId = tenantId, UserId = userId, NoteDate = DateTime.Today, Content = "Met with the dev team to discuss the API migration. Everyone is aligned on the new architecture.", IsPinned = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Note { TenantId = tenantId, UserId = userId, NoteDate = DateTime.Today.AddDays(-1), Content = "Initial research for the new Blazor dashboard. Glassmorphism looks like the way to go.", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        );

        await db.SaveChangesAsync();
        logger.LogInformation("Sample data seeding completed for user: {UserId}", userId);
    }
}
