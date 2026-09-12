using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RestaurantSaaS.Application.Interfaces;
using RestaurantSaaS.Domain.Enums;

namespace RestaurantSaaS.Application.Taxation.Services;

public class TaxCalculationService : ITaxCalculationService
{
    private readonly IApplicationDbContext _context;

    public TaxCalculationService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TaxCalculationResult> CalculateTaxAsync(
        Guid restaurantId,
        Guid? branchId,
        decimal subtotal,
        OrderType orderType,
        PaymentMethod paymentMethod,
        CancellationToken cancellationToken = default)
    {
        // 1. Fetch active rules for the restaurant
        var query = _context.TaxRules
            .Where(r => r.RestaurantId == restaurantId && r.IsActive);

        // Branch-specific or restaurant-wide
        if (branchId.HasValue)
        {
            query = query.Where(r => r.BranchId == null || r.BranchId == branchId.Value);
        }
        else
        {
            query = query.Where(r => r.BranchId == null);
        }

        var rules = await query.ToListAsync(cancellationToken);

        // 2. Filter rules matching OrderType and PaymentMethod
        // Priority: Exact match (both match) -> Payment match -> OrderType match -> Generic rule (neither specified)
        var matchedRule = rules
            .Where(r => (!r.AppliesToOrderType.HasValue || r.AppliesToOrderType == orderType) &&
                        (!r.AppliesToPaymentMethod.HasValue || r.AppliesToPaymentMethod == paymentMethod))
            .OrderByDescending(r => (r.BranchId.HasValue ? 4 : 0) +
                                    (r.AppliesToPaymentMethod.HasValue ? 2 : 0) +
                                    (r.AppliesToOrderType.HasValue ? 1 : 0))
            .FirstOrDefault();

        if (matchedRule != null)
        {
            var taxRate = matchedRule.RatePercentage;
            var taxAmount = Math.Round(subtotal * (taxRate / 100m), 2, MidpointRounding.AwayFromZero);
            var grandTotal = subtotal + taxAmount;

            return new TaxCalculationResult(
                NetAmount: subtotal,
                TaxRatePercentage: taxRate,
                TaxAmount: taxAmount,
                TotalAmount: grandTotal,
                AppliedRuleName: matchedRule.Name
            );
        }

        // Default: 0% tax if no active rule configured
        return new TaxCalculationResult(
            NetAmount: subtotal,
            TaxRatePercentage: 0.00m,
            TaxAmount: 0.00m,
            TotalAmount: subtotal,
            AppliedRuleName: "Default (Zero Tax)"
        );
    }
}
