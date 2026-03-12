namespace DailyNotes.Shared.Models;

public class Project
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Visibility { get; set; } = "private";
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
    public DateTime? CreatedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
