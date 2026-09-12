using RestaurantSaaS.SharedKernel.Common;
using RestaurantSaaS.SharedKernel.Interfaces;
using RestaurantSaaS.Domain.Enums;

namespace RestaurantSaaS.Domain.Entities;

public class FloorSection : BaseEntity, IBranchScopedEntity
{
    public Guid RestaurantId { get; set; }
    public Guid? BranchId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; } = 0;

    // Navigation
    public virtual Branch? Branch { get; set; }
    public virtual ICollection<RestaurantTable> Tables { get; set; } = new List<RestaurantTable>();
}

public class RestaurantTable : BaseEntity, IBranchScopedEntity
{
    public Guid RestaurantId { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? FloorSectionId { get; set; }
    public string TableNumber { get; set; } = string.Empty;
    public int Capacity { get; set; } = 4;
    public TableStatus Status { get; set; } = TableStatus.Available;
    public Guid? CurrentOrderId { get; set; }

    // Navigation
    public virtual Branch? Branch { get; set; }
    public virtual FloorSection? FloorSection { get; set; }
}
