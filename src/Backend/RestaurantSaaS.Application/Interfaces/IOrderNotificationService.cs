using RestaurantSaaS.Application.DTOs;

namespace RestaurantSaaS.Application.Interfaces;

public interface IOrderNotificationService
{
    Task NotifyOrderCreatedAsync(OrderDto order);
    Task NotifyOrderStatusChangedAsync(Guid orderId, string orderNumber, string newStatus, Guid restaurantId, Guid? branchId);
    Task NotifyOrderPaidAsync(Guid orderId, string orderNumber, decimal amount, Guid restaurantId, Guid? branchId);
}
