using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantSaaS.Application.Interfaces;
using RestaurantSaaS.Domain.Enums;
using RestaurantSaaS.Domain.Exceptions;

namespace RestaurantSaaS.Application.Deliveries.Commands;

public record UpdateDeliveryStatusCommand(
    Guid DispatchId,
    DeliveryStatus NewStatus,
    decimal? CashCollected = null,
    string? FailureReason = null
) : IRequest<bool>;

public class UpdateDeliveryStatusCommandValidator : AbstractValidator<UpdateDeliveryStatusCommand>
{
    public UpdateDeliveryStatusCommandValidator()
    {
        RuleFor(x => x.DispatchId).NotEmpty();
        RuleFor(x => x.NewStatus).IsInEnum();
    }
}

public class UpdateDeliveryStatusCommandHandler : IRequestHandler<UpdateDeliveryStatusCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public UpdateDeliveryStatusCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateDeliveryStatusCommand request, CancellationToken cancellationToken)
    {
        var dispatch = await _context.DeliveryDispatches
            .Include(d => d.Order)
            .FirstOrDefaultAsync(d => d.Id == request.DispatchId, cancellationToken)
            ?? throw new NotFoundException($"Delivery dispatch {request.DispatchId} not found.");

        dispatch.Status = request.NewStatus;

        switch (request.NewStatus)
        {
            case DeliveryStatus.Accepted:
                dispatch.AcceptedAt = DateTime.UtcNow;
                break;
            case DeliveryStatus.PickedUp:
                dispatch.PickedUpAt = DateTime.UtcNow;
                if (dispatch.Order != null) dispatch.Order.Status = OrderStatus.OutForDelivery;
                break;
            case DeliveryStatus.Delivered:
                dispatch.DeliveredAt = DateTime.UtcNow;
                if (request.CashCollected.HasValue)
                {
                    dispatch.CashCollected = request.CashCollected.Value;
                    dispatch.IsCashCollected = true;
                }
                if (dispatch.Order != null)
                {
                    dispatch.Order.Status = OrderStatus.Delivered;
                    dispatch.Order.PaymentStatus = PaymentStatus.Paid;
                }
                break;
            case DeliveryStatus.Failed:
                dispatch.FailureReason = request.FailureReason;
                break;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
