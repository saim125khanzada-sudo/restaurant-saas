using RestaurantSaaS.Domain.Enums;

namespace RestaurantSaaS.Domain.Services;

public static class OrderStateMachine
{
    private static readonly Dictionary<OrderStatus, HashSet<OrderStatus>> AllowedTransitions = new()
    {
        [OrderStatus.Created] = new() { OrderStatus.Submitted, OrderStatus.Confirmed, OrderStatus.Cancelled },
        [OrderStatus.Submitted] = new() { OrderStatus.Confirmed, OrderStatus.KitchenPreparing, OrderStatus.Cancelled },
        [OrderStatus.Confirmed] = new() { OrderStatus.KitchenPreparing, OrderStatus.Ready, OrderStatus.ReadyForPickup, OrderStatus.Cancelled },
        [OrderStatus.KitchenPreparing] = new() { OrderStatus.Ready, OrderStatus.ReadyForPickup, OrderStatus.Cancelled },
        [OrderStatus.Ready] = new() { OrderStatus.Served, OrderStatus.Completed, OrderStatus.Paid },
        [OrderStatus.Served] = new() { OrderStatus.Completed, OrderStatus.Paid },
        [OrderStatus.Completed] = new() { OrderStatus.Paid },
        [OrderStatus.Paid] = new() { OrderStatus.Refunded },

        [OrderStatus.ReadyForPickup] = new() { OrderStatus.RiderAssigned, OrderStatus.PickedUp, OrderStatus.Cancelled },
        [OrderStatus.RiderAssigned] = new() { OrderStatus.PickedUp, OrderStatus.Cancelled },
        [OrderStatus.PickedUp] = new() { OrderStatus.OutForDelivery, OrderStatus.Cancelled },
        [OrderStatus.OutForDelivery] = new() { OrderStatus.Delivered, OrderStatus.Cancelled },
        [OrderStatus.Delivered] = new() { OrderStatus.CodSettled, OrderStatus.Paid },
        [OrderStatus.CodSettled] = new() { OrderStatus.Paid },

        [OrderStatus.Cancelled] = new(),
        [OrderStatus.Refunded] = new()
    };

    public static bool CanTransition(OrderStatus currentStatus, OrderStatus targetStatus)
    {
        if (currentStatus == targetStatus) return true;
        return AllowedTransitions.TryGetValue(currentStatus, out var nextAllowed) && nextAllowed.Contains(targetStatus);
    }
}
