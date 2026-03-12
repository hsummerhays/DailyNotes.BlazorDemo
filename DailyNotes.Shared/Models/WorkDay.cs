using System.ComponentModel.DataAnnotations;

namespace DailyNotes.Shared.Models;

public class WorkDay
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime WorkDate { get; set; }
    public DateTime? TimeIn1 { get; set; }
    public DateTime? TimeOut1 { get; set; }
    public DateTime? TimeIn2 { get; set; }
    public DateTime? TimeOut2 { get; set; }
    public DateTime? TimeIn3 { get; set; }
    public DateTime? TimeOut3 { get; set; }
    public int BreakMinutes { get; set; }
    public string? Comments { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
