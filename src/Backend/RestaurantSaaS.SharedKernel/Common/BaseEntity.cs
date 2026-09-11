using RestaurantSaaS.SharedKernel.Interfaces;

namespace RestaurantSaaS.SharedKernel.Common;

public abstract class BaseEntity : IAuditableEntity, ISoftDelete
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string CreatedBy { get; set; } = "SYSTEM";
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTimeOffset? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}
