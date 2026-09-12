using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantSaaS.Application.Interfaces;
using RestaurantSaaS.Domain.Entities;
using RestaurantSaaS.Domain.Enums;
using RestaurantSaaS.Domain.Exceptions;

namespace RestaurantSaaS.Application.Deliveries.Commands;

public record AssignDeliveryCommand(
    Guid OrderId,
    Guid RiderId,
    string DeliveryAddress,
    decimal? DestinationLatitude,
    decimal? DestinationLongitude,
    string? CustomerPhone,
    string? CustomerName,
    decimal CashToCollect
) : IRequest<Guid>;

public class AssignDeliveryCommandValidator : AbstractValidator<AssignDeliveryCommand>
{
    public AssignDeliveryCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.RiderId).NotEmpty();
        RuleFor(x => x.DeliveryAddress).NotEmpty().MaximumLength(500);
        RuleFor(x => x.CashToCollect).GreaterThanOrEqualTo(0);
    }
}

public class AssignDeliveryCommandHandler : IRequestHandler<AssignDeliveryCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenantService _tenantService;

    public AssignDeliveryCommandHandler(IApplicationDbContext context, ICurrentTenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    public async Task<Guid> Handle(AssignDeliveryCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken)
            ?? throw new NotFoundException($"Order {request.OrderId} not found.");

        var rider = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.RiderId, cancellationToken)
            ?? throw new NotFoundException($"Rider {request.RiderId} not found.");

        var dispatch = new DeliveryDispatch
        {
            RestaurantId = _tenantService.RestaurantId ?? order.RestaurantId,
            BranchId = order.BranchId ?? Guid.Empty,
            OrderId = order.Id,
            RiderId = rider.Id,
            Status = DeliveryStatus.Assigned,
            DeliveryAddress = request.DeliveryAddress,
            DestinationLatitude = request.DestinationLatitude,
            DestinationLongitude = request.DestinationLongitude,
            CustomerPhone = request.CustomerPhone,
            CustomerName = request.CustomerName,
            CashToCollect = request.CashToCollect,
            AssignedAt = DateTime.UtcNow
        };

        _context.DeliveryDispatches.Add(dispatch);
        await _context.SaveChangesAsync(cancellationToken);

        return dispatch.Id;
    }
}
