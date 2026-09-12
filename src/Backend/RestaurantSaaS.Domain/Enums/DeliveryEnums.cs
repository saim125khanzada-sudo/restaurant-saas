namespace RestaurantSaaS.Domain.Enums;

public enum DeliveryStatus
{
    Assigned = 1,
    Accepted = 2,
    Rejected = 3,
    PickedUp = 4,
    InTransit = 5,
    Delivered = 6,
    Failed = 7,
    Cancelled = 8
}

public enum RiderAvailabilityStatus
{
    Offline = 1,
    Available = 2,
    Busy = 3
}

public enum ReconciliationStatus
{
    Pending = 1,
    Approved = 2,
    DiscrepancyReported = 3
}
