using Microsoft.EntityFrameworkCore;
using DailyNotes.Shared.Models;

namespace DailyNotes.Api.Data;

public class NotesDbContext : DbContext
{
    public NotesDbContext(DbContextOptions<NotesDbContext> options)
        : base(options)
    {
    }

    public DbSet<Note> Notes { get; set; } = default!;
    public DbSet<WorkDay> WorkDays { get; set; } = default!;
    public DbSet<Project> Projects { get; set; } = default!;
    public DbSet<WorkTask> WorkTasks { get; set; } = default!;
    public DbSet<TenantUser> TenantUsers { get; set; } = default!;
    public DbSet<Topic> Topics { get; set; } = default!;
    public DbSet<TopicNote> TopicNotes { get; set; } = default!;
    public DbSet<Tag> Tags { get; set; } = default!;
    public DbSet<Assignment> Assignments { get; set; } = default!;
}