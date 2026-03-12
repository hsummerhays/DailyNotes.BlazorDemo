namespace DailyNotes.Shared.Interfaces;

/// <summary>
/// Contract for entities that are scoped to a tenant.
/// </summary>
public interface IHasTenant
{
    int TenantId { get; set; }
}
