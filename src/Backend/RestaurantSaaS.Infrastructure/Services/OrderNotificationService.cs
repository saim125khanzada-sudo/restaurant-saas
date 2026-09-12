using Microsoft.AspNetCore.SignalR;
using RestaurantSaaS.Application.DTOs;
using RestaurantSaaS.Application.Interfaces;
using RestaurantSaaS.Infrastructure.Hubs;

namespace RestaurantSaaS.Infrastructure.Services;

public class OrderNotificationService : IOrderNotificationService
{
    private readonly IHubContext<OrderHub> _hubContext;

    public OrderNotificationService(IHubContext<OrderHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyOrderCreatedAsync(OrderDto order)
    {
        if (order.BranchId.HasValue)
        {
            await _hubContext.Clients.Group($"branch_{order.BranchId.Value}")
                .SendAsync("OrderCreated", order);
        }
        else
        {
            await _hubContext.Clients.All.SendAsync("OrderCreated", order);
        }
    }

    public async Task NotifyOrderStatusChangedAsync(Guid orderId, string orderNumber, string newStatus, Guid restaurantId, Guid? branchId)
    {
        var payload = new { orderId, orderNumber, newStatus, restaurantId, branchId, timestamp = DateTimeOffset.UtcNow };
        if (branchId.HasValue)
        {
            await _hubContext.Clients.Group($"branch_{branchId.Value}")
                .SendAsync("OrderStatusChanged", payload);
        }
        else
        {
            await _hubContext.Clients.All.SendAsync("OrderStatusChanged", payload);
        }
    }

    public async Task NotifyOrderPaidAsync(Guid orderId, string orderNumber, decimal amount, Guid restaurantId, Guid? branchId)
    {
        var payload = new { orderId, orderNumber, amount, timestamp = DateTimeOffset.UtcNow };
        if (branchId.HasValue)
        {
            await _hubContext.Clients.Group($"branch_{branchId.Value}")
                .SendAsync("OrderPaid", payload);
        }
        else
        {
            await _hubContext.Clients.All.SendAsync("OrderPaid", payload);
        }
    }
}
