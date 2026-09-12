using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantSaaS.Application.DTOs;
using RestaurantSaaS.Application.Interfaces;
using RestaurantSaaS.Domain.Entities;
using RestaurantSaaS.Domain.Enums;
using RestaurantSaaS.SharedKernel.Common;

namespace RestaurantSaaS.Application.Features.Payments.Commands;

public record ProcessPaymentCommand(ProcessPaymentRequest Request) : IRequest<Result<PaymentDto>>;

public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, Result<PaymentDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenantService _tenantService;
    private readonly IOrderNotificationService _notificationService;

    public ProcessPaymentCommandHandler(
        IApplicationDbContext context,
        ICurrentTenantService tenantService,
        IOrderNotificationService notificationService)
    {
        _context = context;
        _tenantService = tenantService;
        _notificationService = notificationService;
    }

    public async Task<Result<PaymentDto>> Handle(ProcessPaymentCommand command, CancellationToken cancellationToken)
    {
        var req = command.Request;
        var order = await _context.Orders
            .Include(o => o.Payments)
            .Include(o => o.Table)
            .FirstOrDefaultAsync(o => o.Id == req.OrderId, cancellationToken);

        if (order == null)
            return Result<PaymentDto>.Failure("Order not found.");

        if (order.PaymentStatus == PaymentStatus.Paid)
            return Result<PaymentDto>.Failure("Order is already fully paid.");

        var payment = new Payment
        {
            RestaurantId = order.RestaurantId,
            BranchId = order.BranchId,
            OrderId = order.Id,
            Amount = req.Amount,
            Method = req.Method,
            TransactionReference = req.TransactionReference?.Trim(),
            ProcessedAt = DateTimeOffset.UtcNow
        };

        order.Payments.Add(payment);

        var totalPaid = order.Payments.Sum(p => p.Amount);
        if (totalPaid >= order.GrandTotal)
        {
            order.PaymentStatus = PaymentStatus.Paid;
            if (order.Status == OrderStatus.Served || order.Status == OrderStatus.Completed)
            {
                order.Status = OrderStatus.Paid;
            }

            if (order.Table != null)
            {
                order.Table.Status = TableStatus.Available;
                order.Table.CurrentOrderId = null;
            }
        }
        else
        {
            order.PaymentStatus = PaymentStatus.Partial;
        }

        await _context.SaveChangesAsync(cancellationToken);

        await _notificationService.NotifyOrderPaidAsync(
            order.Id,
            order.OrderNumber,
            req.Amount,
            order.RestaurantId,
            order.BranchId
        );

        return Result<PaymentDto>.Success(new PaymentDto(
            payment.Id,
            payment.OrderId,
            payment.Amount,
            payment.Method,
            payment.TransactionReference,
            payment.ProcessedAt
        ));
    }
}
