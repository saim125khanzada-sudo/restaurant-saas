namespace RestaurantSaaS.Domain.Enums;

public enum UserStatus
{
    Active = 1,
    Suspended = 2,
    LockedOut = 3,
    Disabled = 4
}

public enum TenantStatus
{
    Active = 1,
    Suspended = 2,
    PendingSetup = 3,
    Terminated = 4
}
