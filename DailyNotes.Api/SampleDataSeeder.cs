using DailyNotes.Api.Data;
using DailyNotes.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace DailyNotes.Api.Data;

public static class SampleDataSeeder
{
    public static async Task<(int TenantId, string UserId)> SeedForUser(NotesDbContext db, string userId, int defaultTenantId = 1)
    {
        var tenantUser = await db.TenantUsers.FirstOrDefaultAsync(tu => tu.UserId == userId);

        if (tenantUser == null)
        {
            Console.WriteLine($"[SEEDER] Creating new TenantUser for {userId}");
            tenantUser = new TenantUser 
            { 
                TenantId = defaultTenantId, 
                UserId = userId, 
                Role = "admin", 
                CreatedAt = DateTime.UtcNow 
            };
            db.TenantUsers.Add(tenantUser);
            await db.SaveChangesAsync();
        }

        var projectCount = await db.Projects.CountAsync(p => p.UserId == userId);
        if (projectCount > 0)
        {
            return (tenantUser.TenantId, userId);
        }

        Console.WriteLine($"[SEEDER] Starting sample data seeding for user: {userId}");

        // Seed Projects
        var productLaunch = new Project { TenantId = tenantUser.TenantId, UserId = userId, Name = "Product Launch 2024", Category = "Marketing", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var infraMigration = new Project { TenantId = tenantUser.TenantId, UserId = userId, Name = "Infrastructure Migration", Category = "IT", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        db.Projects.AddRange(productLaunch, infraMigration);
        await db.SaveChangesAsync(); // Need IDs for tasks

        // Seed Work Days
        var today = new WorkDay { TenantId = tenantUser.TenantId, UserId = userId, WorkDate = DateTime.Today, TimeIn1 = DateTime.Today.AddHours(9), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        db.WorkDays.Add(today);

        // Seed Tasks
        db.WorkTasks.AddRange(
            new WorkTask { TenantId = tenantUser.TenantId, UserId = userId, ProjectId = productLaunch.Id, Name = "Draft social media announcement", Status = "todo", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new WorkTask { TenantId = tenantUser.TenantId, UserId = userId, ProjectId = productLaunch.Id, Name = "Prepare slide deck for kickoff", Status = "in-progress", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new WorkTask { TenantId = tenantUser.TenantId, UserId = userId, ProjectId = infraMigration.Id, Name = "Audit cloud resource usage", Status = "done", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        );

        // Seed Notes
        db.Notes.AddRange(
            new Note { TenantId = tenantUser.TenantId, UserId = userId, NoteDate = DateTime.Today, Content = "Met with the dev team to discuss the API migration. Everyone is aligned on the new architecture.", IsPinned = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Note { TenantId = tenantUser.TenantId, UserId = userId, NoteDate = DateTime.Today.AddDays(-1), Content = "Initial research for the new Blazor dashboard. Glassmorphism looks like the way to go.", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        );

        await db.SaveChangesAsync();
        Console.WriteLine($"[SEEDER] Sample data seeding completed for user: {userId}");

        return (tenantUser.TenantId, userId);
    }
}
