namespace RestaurantSaaS.Domain.Enums;

public enum OrderType
{
    DineIn = 1,
    Takeaway = 2,
    Delivery = 3
}

public enum OrderStatus
{
    Created = 1,
    Submitted = 2,
    Confirmed = 3,
    KitchenPreparing = 4,
    Ready = 5,
    Served = 6,
    Completed = 7,
    Paid = 8,
    Cancelled = 9,
    Refunded = 10,
    ReadyForPickup = 11,
    RiderAssigned = 12,
    PickedUp = 13,
    OutForDelivery = 14,
    Delivered = 15,
    CodSettled = 16
}

public enum PaymentMethod
{
    Cash = 1,
    Card = 2,
    BankTransfer = 3
}

public enum PaymentStatus
{
    Unpaid = 1,
    Partial = 2,
    Paid = 3,
    Refunded = 4
}
