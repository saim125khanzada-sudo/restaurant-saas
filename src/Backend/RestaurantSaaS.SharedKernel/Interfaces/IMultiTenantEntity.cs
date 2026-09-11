namespace RestaurantSaaS.SharedKernel.Interfaces;

/// <summary>
/// Enforces mandatory tenant isolation at the database and application levels.
/// Every tenant-scoped entity MUST implement this interface.
/// </summary>
public interface IMultiTenantEntity
{
    Guid RestaurantId { get; set; }
}
