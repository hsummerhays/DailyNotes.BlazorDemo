namespace DailyNotes.Shared.Models;

public class Note
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Visibility { get; set; } = "private";
    public int? WorkTaskId { get; set; }
    public DateTime NoteDate { get; set; }
    public string Content { get; set; } = string.Empty; // JSON stored as TEXT
    public int TimeMinutes { get; set; }
    public string? ExternalSource { get; set; }
    public string? ExternalId { get; set; }
    public bool IsPinned { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
