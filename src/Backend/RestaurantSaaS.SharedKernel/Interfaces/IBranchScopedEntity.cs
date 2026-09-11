namespace RestaurantSaaS.SharedKernel.Interfaces;

/// <summary>
/// Applied to entities that belong to a specific operational branch of a restaurant.
/// </summary>
public interface IBranchScopedEntity : IMultiTenantEntity
{
    Guid? BranchId { get; set; }
}
