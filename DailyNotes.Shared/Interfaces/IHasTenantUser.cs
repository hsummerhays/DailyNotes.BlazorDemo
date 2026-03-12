namespace DailyNotes.Shared.Interfaces;

/// <summary>
/// Marker + contract for entities that are scoped to a tenant and user.
/// Implement on entities that expose both TenantId and UserId.
/// </summary>
public interface IHasTenantUser : IHasTenant
{
    string UserId { get; set; }
}
