using Microsoft.EntityFrameworkCore;

namespace DailyNotes.Shared.Models;

[PrimaryKey(nameof(TenantId), nameof(UserId))]
public class TenantUser
{
    public int TenantId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Role { get; set; } = "user";
    public string Preferences { get; set; } = "{}";
    public DateTime CreatedAt { get; set; }
}
