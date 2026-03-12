namespace DailyNotes.Shared.Models;

public class WorkTask
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Visibility { get; set; } = "private";
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = "todo";
    public string? Comments { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }
    public int? ProjectId { get; set; }
    public int? ParentTaskId { get; set; }
    public string? ExternalSource { get; set; }
    public string? ExternalId { get; set; }
    public bool IsPinned { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
