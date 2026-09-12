using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantSaaS.Application.Interfaces;
using RestaurantSaaS.Domain.Enums;

namespace RestaurantSaaS.Application.Taxation.Queries;

public record TaxRuleDto(
    Guid Id,
    Guid RestaurantId,
    Guid? BranchId,
    string Name,
    decimal RatePercentage,
    OrderType? AppliesToOrderType,
    PaymentMethod? AppliesToPaymentMethod,
    bool IsActive,
    string? Description
);

public record GetTaxRulesQuery(Guid RestaurantId, Guid? BranchId = null) : IRequest<List<TaxRuleDto>>;

public class GetTaxRulesQueryHandler : IRequestHandler<GetTaxRulesQuery, List<TaxRuleDto>>
{
    private readonly IApplicationDbContext _context;

    public GetTaxRulesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TaxRuleDto>> Handle(GetTaxRulesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.TaxRules
            .Where(r => r.RestaurantId == request.RestaurantId);

        if (request.BranchId.HasValue)
        {
            query = query.Where(r => r.BranchId == null || r.BranchId == request.BranchId.Value);
        }

        return await query
            .OrderBy(r => r.Name)
            .Select(r => new TaxRuleDto(
                r.Id,
                r.RestaurantId,
                r.BranchId,
                r.Name,
                r.RatePercentage,
                r.AppliesToOrderType,
                r.AppliesToPaymentMethod,
                r.IsActive,
                r.Description
            ))
            .ToListAsync(cancellationToken);
    }
}

public record TaxSummaryReportDto(
    Guid RestaurantId,
    Guid? BranchId,
    int TotalFiscalizedInvoices,
    int PendingOrOfflineInvoices,
    decimal TotalSalesCalculated,
    decimal TotalTaxCollected
);

public record GetTaxSummaryQuery(Guid RestaurantId, Guid? BranchId = null) : IRequest<TaxSummaryReportDto>;

public class GetTaxSummaryQueryHandler : IRequestHandler<GetTaxSummaryQuery, TaxSummaryReportDto>
{
    private readonly IApplicationDbContext _context;

    public GetTaxSummaryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TaxSummaryReportDto> Handle(GetTaxSummaryQuery request, CancellationToken cancellationToken)
    {
        var query = _context.FiscalInvoiceRecords
            .Where(f => f.RestaurantId == request.RestaurantId);

        if (request.BranchId.HasValue)
        {
            query = query.Where(f => f.BranchId == request.BranchId.Value);
        }

        var records = await query.ToListAsync(cancellationToken);

        var totalSuccess = records.Count(r => r.Status == FiscalSyncStatus.Success);
        var totalPending = records.Count(r => r.Status != FiscalSyncStatus.Success);
        var totalSales = records.Where(r => r.Status == FiscalSyncStatus.Success).Sum(r => r.TotalSalesValue);
        var totalTax = records.Where(r => r.Status == FiscalSyncStatus.Success).Sum(r => r.TotalTaxCharged);

        return new TaxSummaryReportDto(
            request.RestaurantId,
            request.BranchId,
            totalSuccess,
            totalPending,
            totalSales,
            totalTax
        );
    }
}
