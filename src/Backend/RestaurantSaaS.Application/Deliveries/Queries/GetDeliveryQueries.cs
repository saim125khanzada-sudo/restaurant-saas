using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantSaaS.Application.Interfaces;
using RestaurantSaaS.Domain.Enums;

namespace RestaurantSaaS.Application.Deliveries.Queries;

public record DeliveryDispatchDto(
    Guid Id,
    Guid OrderId,
    string OrderNumber,
    Guid RiderId,
    DeliveryStatus Status,
    string DeliveryAddress,
    decimal? DestinationLatitude,
    decimal? DestinationLongitude,
    string? CustomerPhone,
    string? CustomerName,
    decimal CashToCollect,
    decimal CashCollected,
    bool IsCashCollected,
    DateTime? AssignedAt,
    DateTime? AcceptedAt,
    DateTime? PickedUpAt,
    DateTime? DeliveredAt
);

public record GetRiderActiveDeliveriesQuery(Guid RiderId) : IRequest<List<DeliveryDispatchDto>>;

public class GetRiderActiveDeliveriesQueryHandler : IRequestHandler<GetRiderActiveDeliveriesQuery, List<DeliveryDispatchDto>>
{
    private readonly IApplicationDbContext _context;

    public GetRiderActiveDeliveriesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<DeliveryDispatchDto>> Handle(GetRiderActiveDeliveriesQuery request, CancellationToken cancellationToken)
    {
        var activeStatuses = new[]
        {
            DeliveryStatus.Assigned,
            DeliveryStatus.Accepted,
            DeliveryStatus.PickedUp,
            DeliveryStatus.InTransit
        };

        var dispatches = await _context.DeliveryDispatches
            .Include(d => d.Order)
            .Where(d => d.RiderId == request.RiderId && activeStatuses.Contains(d.Status))
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(cancellationToken);

        return dispatches.Select(d => new DeliveryDispatchDto(
            d.Id,
            d.OrderId,
            d.Order?.OrderNumber ?? "N/A",
            d.RiderId,
            d.Status,
            d.DeliveryAddress,
            d.DestinationLatitude,
            d.DestinationLongitude,
            d.CustomerPhone,
            d.CustomerName,
            d.CashToCollect,
            d.CashCollected,
            d.IsCashCollected,
            d.AssignedAt,
            d.AcceptedAt,
            d.PickedUpAt,
            d.DeliveredAt
        )).ToList();
    }
}
