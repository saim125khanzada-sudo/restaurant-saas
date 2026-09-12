using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantSaaS.Application.DTOs;
using RestaurantSaaS.Application.Interfaces;
using RestaurantSaaS.Domain.Entities;
using RestaurantSaaS.Domain.Enums;
using RestaurantSaaS.Domain.Services;
using RestaurantSaaS.SharedKernel.Common;

namespace RestaurantSaaS.Application.Features.Orders.Commands;

public record TransitionOrderStatusCommand(TransitionOrderStatusRequest Request) : IRequest<Result<OrderStatus>>;

public class TransitionOrderStatusCommandHandler : IRequestHandler<TransitionOrderStatusCommand, Result<OrderStatus>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenantService _tenantService;
    private readonly IOrderNotificationService _notificationService;

    public TransitionOrderStatusCommandHandler(
        IApplicationDbContext context,
        ICurrentTenantService tenantService,
        IOrderNotificationService notificationService)
    {
        _context = context;
        _tenantService = tenantService;
        _notificationService = notificationService;
    }

    public async Task<Result<OrderStatus>> Handle(TransitionOrderStatusCommand command, CancellationToken cancellationToken)
    {
        var req = command.Request;
        var order = await _context.Orders
            .Include(o => o.Table)
            .FirstOrDefaultAsync(o => o.Id == req.OrderId, cancellationToken);

        if (order == null)
            return Result<OrderStatus>.Failure("Order not found.");

        if (!OrderStateMachine.CanTransition(order.Status, req.TargetStatus))
        {
            return Result<OrderStatus>.Failure($"Invalid status transition from '{order.Status}' to '{req.TargetStatus}'.");
        }

        var previousStatus = order.Status;
        order.Status = req.TargetStatus;

        // Log to immutable history
        _context.OrderStatusHistories.Add(new OrderStatusHistory
        {
            RestaurantId = order.RestaurantId,
            BranchId = order.BranchId,
            OrderId = order.Id,
            PreviousStatus = previousStatus,
            NewStatus = req.TargetStatus,
            ChangedByUserId = _tenantService.UserId,
            Reason = req.Reason
        });

        // Release table if completed or cancelled
        if ((req.TargetStatus == OrderStatus.Completed || req.TargetStatus == OrderStatus.Cancelled) && order.Table != null)
        {
            order.Table.Status = TableStatus.Available;
            order.Table.CurrentOrderId = null;
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Broadcast real-time status change to POS, Waiters, and Riders
        await _notificationService.NotifyOrderStatusChangedAsync(
            order.Id,
            order.OrderNumber,
            req.TargetStatus.ToString(),
            order.RestaurantId,
            order.BranchId
        );

        return Result<OrderStatus>.Success(order.Status);
    }
}
