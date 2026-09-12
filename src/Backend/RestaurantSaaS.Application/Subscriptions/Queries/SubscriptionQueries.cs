using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantSaaS.Application.Interfaces;
using RestaurantSaaS.Domain.Entities;
using RestaurantSaaS.Domain.Exceptions;

namespace RestaurantSaaS.Application.Subscriptions.Queries;

public record SubscriptionPlanDto(
    Guid Id,
    string Name,
    decimal MonthlyPrice,
    int MaxBranches,
    int MaxUsersPerBranch,
    bool HasAdvancedAnalytics,
    bool HasFbrIntegration,
    bool IsActive
);

public record GetSubscriptionPlansQuery : IRequest<List<SubscriptionPlanDto>>;

public class GetSubscriptionPlansQueryHandler : IRequestHandler<GetSubscriptionPlansQuery, List<SubscriptionPlanDto>>
{
    private readonly IApplicationDbContext _context;

    public GetSubscriptionPlansQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<SubscriptionPlanDto>> Handle(GetSubscriptionPlansQuery request, CancellationToken cancellationToken)
    {
        return await _context.SubscriptionPlans
            .Where(p => p.IsActive)
            .OrderBy(p => p.MonthlyPrice)
            .Select(p => new SubscriptionPlanDto(
                p.Id,
                p.Name,
                p.MonthlyPrice,
                p.MaxBranches,
                p.MaxUsersPerBranch,
                p.HasAdvancedAnalytics,
                p.HasFbrIntegration,
                p.IsActive
            ))
            .ToListAsync(cancellationToken);
    }
}

public record TenantSubscriptionStatusDto(
    Guid RestaurantId,
    string PlanName,
    decimal MonthlyPrice,
    int CurrentBranchCount,
    int MaxAllowedBranches,
    bool CanCreateMoreBranches,
    SubscriptionStatus Status,
    DateTime PeriodEnd,
    bool AutoRenew
);

public record GetTenantSubscriptionStatusQuery(Guid RestaurantId) : IRequest<TenantSubscriptionStatusDto>;

public class GetTenantSubscriptionStatusQueryHandler : IRequestHandler<GetTenantSubscriptionStatusQuery, TenantSubscriptionStatusDto>
{
    private readonly IApplicationDbContext _context;

    public GetTenantSubscriptionStatusQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TenantSubscriptionStatusDto> Handle(GetTenantSubscriptionStatusQuery request, CancellationToken cancellationToken)
    {
        var subscription = await _context.TenantSubscriptions
            .Include(s => s.Plan)
            .Where(s => s.RestaurantId == request.RestaurantId && s.Status == SubscriptionStatus.Active)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var branchCount = await _context.Branches
            .CountAsync(b => b.RestaurantId == request.RestaurantId, cancellationToken);

        if (subscription == null)
        {
            // Default to Free Trial
            return new TenantSubscriptionStatusDto(
                RestaurantId: request.RestaurantId,
                PlanName: "Free Trial (Default)",
                MonthlyPrice: 0.00m,
                CurrentBranchCount: branchCount,
                MaxAllowedBranches: 1,
                CanCreateMoreBranches: branchCount < 1,
                Status: SubscriptionStatus.Trial,
                PeriodEnd: DateTime.UtcNow.AddDays(14),
                AutoRenew: false
            );
        }

        return new TenantSubscriptionStatusDto(
            RestaurantId: request.RestaurantId,
            PlanName: subscription.Plan.Name,
            MonthlyPrice: subscription.Plan.MonthlyPrice,
            CurrentBranchCount: branchCount,
            MaxAllowedBranches: subscription.Plan.MaxBranches,
            CanCreateMoreBranches: branchCount < subscription.Plan.MaxBranches,
            Status: subscription.Status,
            PeriodEnd: subscription.PeriodEnd,
            AutoRenew: subscription.AutoRenew
        );
    }
}
