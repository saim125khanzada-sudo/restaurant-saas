using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantSaaS.Application.DTOs;
using RestaurantSaaS.Application.Interfaces;
using RestaurantSaaS.Domain.Entities;
using RestaurantSaaS.Domain.Enums;
using RestaurantSaaS.SharedKernel.Common;

namespace RestaurantSaaS.Application.Features.Orders.Commands;

public record CreateOrderCommand(CreateOrderRequest Request) : IRequest<Result<OrderDto>>;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<OrderDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenantService _tenantService;
    private readonly IOrderNotificationService _notificationService;

    public CreateOrderCommandHandler(
        IApplicationDbContext context,
        ICurrentTenantService tenantService,
        IOrderNotificationService notificationService)
    {
        _context = context;
        _tenantService = tenantService;
        _notificationService = notificationService;
    }

    public async Task<Result<OrderDto>> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        if (!_tenantService.RestaurantId.HasValue)
            return Result<OrderDto>.Failure("Tenant context is required.");

        var req = command.Request;
        var restaurantId = _tenantService.RestaurantId.Value;
        var branchId = req.BranchId ?? _tenantService.BranchId;

        // IDEMPOTENCY CHECK: If operation was already processed, return existing order to prevent duplicate billing
        var existingOrder = await _context.Orders
            .Include(o => o.Table)
            .Include(o => o.Waiter)
            .Include(o => o.Items)
                .ThenInclude(i => i.Addons)
            .FirstOrDefaultAsync(o => o.IdempotencyKey == req.IdempotencyKey, cancellationToken);

        if (existingOrder != null)
        {
            return Result<OrderDto>.Success(MapToDto(existingOrder));
        }

        if (req.Items == null || req.Items.Count == 0)
            return Result<OrderDto>.Failure("Order must contain at least one item.");

        // Generate unique Order Number
        var todayCount = await _context.Orders.CountAsync(o => o.CreatedAt >= DateTimeOffset.UtcNow.Date, cancellationToken);
        var orderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{(todayCount + 1):D4}";

        var order = new Order
        {
            RestaurantId = restaurantId,
            BranchId = branchId,
            OrderNumber = orderNumber,
            OrderType = req.OrderType,
            Status = OrderStatus.Created,
            PaymentStatus = PaymentStatus.Unpaid,
            TableId = req.TableId,
            WaiterId = req.WaiterId ?? _tenantService.UserId,
            CustomerName = req.CustomerName?.Trim(),
            CustomerPhone = req.CustomerPhone?.Trim(),
            DeliveryAddress = req.DeliveryAddress?.Trim(),
            DiscountTotal = req.DiscountTotal,
            DiscountReason = req.DiscountReason?.Trim(),
            IdempotencyKey = req.IdempotencyKey
        };

        decimal subtotal = 0;
        foreach (var itemReq in req.Items)
        {
            var itemTotal = itemReq.UnitPrice * itemReq.Quantity;
            var orderItem = new OrderItem
            {
                RestaurantId = restaurantId,
                ProductId = itemReq.ProductId,
                ProductVariantId = itemReq.ProductVariantId,
                ItemName = itemReq.ItemName.Trim(),
                Quantity = itemReq.Quantity,
                UnitPrice = itemReq.UnitPrice,
                TotalPrice = itemTotal,
                KitchenNotes = itemReq.KitchenNotes?.Trim()
            };

            if (itemReq.Addons != null)
            {
                foreach (var addonReq in itemReq.Addons)
                {
                    var addonTotal = addonReq.UnitPrice * addonReq.Quantity;
                    itemTotal += addonTotal;
                    orderItem.Addons.Add(new OrderItemAddon
                    {
                        RestaurantId = restaurantId,
                        AddonId = addonReq.AddonId,
                        AddonName = addonReq.AddonName.Trim(),
                        UnitPrice = addonReq.UnitPrice,
                        Quantity = addonReq.Quantity,
                        TotalPrice = addonTotal
                    });
                }
            }

            orderItem.TotalPrice = itemTotal;
            subtotal += itemTotal;
            order.Items.Add(orderItem);
        }

        order.Subtotal = subtotal;
        order.TaxTotal = Math.Round(subtotal * 0.05m, 2); // Standard baseline sales tax
        order.GrandTotal = Math.Max(0, (order.Subtotal + order.TaxTotal) - order.DiscountTotal);

        // Track History
        order.StatusHistory.Add(new OrderStatusHistory
        {
            RestaurantId = restaurantId,
            BranchId = branchId,
            PreviousStatus = OrderStatus.Created,
            NewStatus = OrderStatus.Created,
            ChangedByUserId = _tenantService.UserId,
            Reason = "Order initiated"
        });

        // If table assigned, mark table as Occupied
        if (order.TableId.HasValue)
        {
            var table = await _context.RestaurantTables.FindAsync(new object[] { order.TableId.Value }, cancellationToken);
            if (table != null)
            {
                table.Status = TableStatus.Occupied;
                table.CurrentOrderId = order.Id;
            }
        }

        _context.Orders.Add(order);
        await _context.SaveChangesAsync(cancellationToken);

        var orderDto = MapToDto(order);

        // Real-time broadcast to POS & Kitchen Display System
        await _notificationService.NotifyOrderCreatedAsync(orderDto);

        return Result<OrderDto>.Success(orderDto);
    }

    private static OrderDto MapToDto(Order o) => new(
        o.Id,
        o.RestaurantId,
        o.BranchId,
        o.OrderNumber,
        o.OrderType,
        o.Status,
        o.PaymentStatus,
        o.TableId,
        o.Table?.TableNumber,
        o.WaiterId,
        o.Waiter?.FullName,
        o.CustomerName,
        o.CustomerPhone,
        o.Subtotal,
        o.TaxTotal,
        o.DiscountTotal,
        o.GrandTotal,
        o.CreatedAt,
        o.Items.Select(i => new OrderItemDto(
            i.Id,
            i.ProductId,
            i.ItemName,
            i.Quantity,
            i.UnitPrice,
            i.TotalPrice,
            i.KitchenNotes,
            i.Addons.Select(a => new OrderItemAddonRequest(a.AddonId, a.AddonName, a.UnitPrice, a.Quantity)).ToList()
        )).ToList()
    );
}
