namespace RestaurantSaaS.Domain.Enums;

public enum StockMovementType
{
    OpeningBalance = 1,
    PurchaseReceipt = 2,
    OrderConsumption = 3,
    Waste = 4,
    Adjustment = 5,
    TransferIn = 6,
    TransferOut = 7
}

public enum PurchaseOrderStatus
{
    Draft = 1,
    Submitted = 2,
    PartiallyReceived = 3,
    Received = 4,
    Cancelled = 5
}
