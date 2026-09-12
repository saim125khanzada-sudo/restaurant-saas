using RestaurantSaaS.SharedKernel.Common;
using RestaurantSaaS.SharedKernel.Interfaces;
using RestaurantSaaS.Domain.Enums;

namespace RestaurantSaaS.Domain.Entities;

public class User : BaseEntity, IBranchScopedEntity
{
    public Guid RestaurantId { get; set; }
    public Guid? BranchId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string NormalizedEmail { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public UserStatus Status { get; set; } = UserStatus.Active;

    // Security & MFA
    public bool IsMfaEnabled { get; set; } = false;
    public string? MfaSecret { get; set; }
    public int FailedLoginAttempts { get; set; } = 0;
    public DateTimeOffset? LockoutEnd { get; set; }
    public DateTimeOffset? LastLoginAt { get; set; }

    // Navigation
    public virtual Restaurant Restaurant { get; set; } = null!;
    public virtual Branch? Branch { get; set; }
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public virtual ICollection<DeviceSession> DeviceSessions { get; set; } = new List<DeviceSession>();
}
