using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantSaaS.Application.Interfaces;
using RestaurantSaaS.Domain.Entities;
using RestaurantSaaS.Domain.Enums;

namespace RestaurantSaaS.Application.Deliveries.Commands;

public record ReconcileRiderCashCommand(
    Guid RiderId,
    Guid BranchId,
    DateTime ShiftDate,
    decimal TotalCashSubmitted,
    string? Notes
) : IRequest<Guid>;

public class ReconcileRiderCashCommandValidator : AbstractValidator<ReconcileRiderCashCommand>
{
    public ReconcileRiderCashCommandValidator()
    {
        RuleFor(x => x.RiderId).NotEmpty();
        RuleFor(x => x.BranchId).NotEmpty();
        RuleFor(x => x.TotalCashSubmitted).GreaterThanOrEqualTo(0);
    }
}

public class ReconcileRiderCashCommandHandler : IRequestHandler<ReconcileRiderCashCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenantService _tenantService;

    public ReconcileRiderCashCommandHandler(IApplicationDbContext context, ICurrentTenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    public async Task<Guid> Handle(ReconcileRiderCashCommand request, CancellationToken cancellationToken)
    {
        var shiftStart = request.ShiftDate.Date;
        var shiftEnd = shiftStart.AddDays(1);

        // Sum cash collected across all completed deliveries for this rider & shift
        var dispatches = await _context.DeliveryDispatches
            .Where(d => d.RiderId == request.RiderId &&
                        d.Status == DeliveryStatus.Delivered &&
                        d.DeliveredAt >= shiftStart &&
                        d.DeliveredAt < shiftEnd)
            .ToListAsync(cancellationToken);

        var totalDeliveries = dispatches.Count;
        var totalExpected = dispatches.Sum(d => d.CashCollected);
        var discrepancy = request.TotalCashSubmitted - totalExpected;

        var recon = new RiderCashReconciliation
        {
            RestaurantId = _tenantService.RestaurantId ?? Guid.Empty,
            BranchId = request.BranchId,
            RiderId = request.RiderId,
            ShiftDate = request.ShiftDate.Date,
            TotalDeliveries = totalDeliveries,
            TotalCashExpected = totalExpected,
            TotalCashSubmitted = request.TotalCashSubmitted,
            DiscrepancyAmount = discrepancy,
            Status = discrepancy == 0 ? ReconciliationStatus.Approved : ReconciliationStatus.DiscrepancyReported,
            Notes = request.Notes
        };

        _context.RiderCashReconciliations.Add(recon);
        await _context.SaveChangesAsync(cancellationToken);

        return recon.Id;
    }
}
