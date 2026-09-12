using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantSaaS.Application.Interfaces;
using RestaurantSaaS.Domain.Entities;
using RestaurantSaaS.Domain.Exceptions;

namespace RestaurantSaaS.Application.Subscriptions.Commands;

// 1. Create/Seed Subscription Plan
public record CreateSubscriptionPlanCommand(
    string Name,
    decimal MonthlyPrice,
    int MaxBranches,
    int MaxUsersPerBranch,
    bool HasAdvancedAnalytics,
    bool HasFbrIntegration
) : IRequest<Guid>;

public class CreateSubscriptionPlanCommandHandler : IRequestHandler<CreateSubscriptionPlanCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateSubscriptionPlanCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateSubscriptionPlanCommand request, CancellationToken cancellationToken)
    {
        var plan = new SubscriptionPlan
        {
            Name = request.Name,
            MonthlyPrice = request.MonthlyPrice,
            MaxBranches = request.MaxBranches,
            MaxUsersPerBranch = request.MaxUsersPerBranch,
            HasAdvancedAnalytics = request.HasAdvancedAnalytics,
            HasFbrIntegration = request.HasFbrIntegration,
            IsActive = true
        };

        _context.SubscriptionPlans.Add(plan);
        await _context.SaveChangesAsync(cancellationToken);
        return plan.Id;
    }
}

// 2. Subscribe Tenant to Plan
public record SubscribeTenantCommand(
    Guid RestaurantId,
    Guid PlanId
) : IRequest<Guid>;

public class SubscribeTenantCommandHandler : IRequestHandler<SubscribeTenantCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public SubscribeTenantCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(SubscribeTenantCommand request, CancellationToken cancellationToken)
    {
        var plan = await _context.SubscriptionPlans
            .FirstOrDefaultAsync(p => p.Id == request.PlanId, cancellationToken);

        if (plan == null)
            throw new NotFoundException($"Subscription Plan {request.PlanId} not found.");

        // Check if tenant has existing branches violating new plan limits
        var branchCount = await _context.Branches
            .CountAsync(b => b.RestaurantId == request.RestaurantId, cancellationToken);

        if (branchCount > plan.MaxBranches)
        {
            throw new DomainException($"Cannot subscribe to {plan.Name} plan. Tenant has {branchCount} branches, exceeding plan maximum of {plan.MaxBranches}.");
        }

        // Deactivate old active subscriptions
        var activeSubs = await _context.TenantSubscriptions
            .Where(s => s.RestaurantId == request.RestaurantId && s.Status == SubscriptionStatus.Active)
            .ToListAsync(cancellationToken);

        foreach (var sub in activeSubs)
        {
            sub.Status = SubscriptionStatus.Cancelled;
        }

        var newSub = new TenantSubscription
        {
            RestaurantId = request.RestaurantId,
            PlanId = plan.Id,
            Status = SubscriptionStatus.Active,
            PeriodStart = DateTime.UtcNow,
            PeriodEnd = DateTime.UtcNow.AddMonths(1),
            AutoRenew = true
        };

        _context.TenantSubscriptions.Add(newSub);

        // Generate initial invoice
        var invoice = new SubscriptionInvoice
        {
            RestaurantId = request.RestaurantId,
            TenantSubscriptionId = newSub.Id,
            InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMM}-{newSub.Id.ToString().Substring(0, 6).ToUpperInvariant()}",
            Amount = plan.MonthlyPrice,
            DueDate = DateTime.UtcNow.AddDays(7),
            Status = InvoiceStatus.Paid,
            PaidAt = DateTime.UtcNow,
            PaymentReference = "AUTOPAY-SIMULATED"
        };

        _context.SubscriptionInvoices.Add(invoice);
        await _context.SaveChangesAsync(cancellationToken);

        return newSub.Id;
    }
}
