using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantSaaS.Application.DTOs;
using RestaurantSaaS.Application.Interfaces;
using RestaurantSaaS.Domain.Entities;
using RestaurantSaaS.SharedKernel.Common;

namespace RestaurantSaaS.Application.Features.Branches.Commands;

public record CreateBranchCommand(CreateBranchRequest Request) : IRequest<Result<BranchDto>>;
public class CreateBranchCommandHandler : IRequestHandler<CreateBranchCommand, Result<BranchDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenantService _tenantService;

    public CreateBranchCommandHandler(IApplicationDbContext context, ICurrentTenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    public async Task<Result<BranchDto>> Handle(CreateBranchCommand command, CancellationToken cancellationToken)
    {
        if (!_tenantService.RestaurantId.HasValue)
            return Result<BranchDto>.Failure("Tenant context is required.");

        var restaurantId = _tenantService.RestaurantId.Value;

        // Phase 10: Enforce Tenant Subscription Branch Limits
        var activeSub = await _context.TenantSubscriptions
            .Include(s => s.Plan)
            .Where(s => s.RestaurantId == restaurantId && s.Status == SubscriptionStatus.Active)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var maxAllowedBranches = activeSub?.Plan.MaxBranches ?? 1; // Default to 1 for Free Trial
        var currentBranchCount = await _context.Branches.CountAsync(b => b.RestaurantId == restaurantId, cancellationToken);

        if (currentBranchCount >= maxAllowedBranches)
        {
            var planName = activeSub?.Plan.Name ?? "Free Trial";
            return Result<BranchDto>.Failure($"Branch limit reached ({currentBranchCount}/{maxAllowedBranches}) for plan '{planName}'. Please upgrade your subscription.");
        }

        var req = command.Request;
        var normalizedCode = req.BranchCode.Trim().ToUpperInvariant();

        if (await _context.Branches.AnyAsync(b => b.BranchCode == normalizedCode, cancellationToken))
            return Result<BranchDto>.Failure($"Branch code '{req.BranchCode}' already exists.");

        var branch = new Branch
        {
            RestaurantId = _tenantService.RestaurantId.Value,
            BranchName = req.BranchName.Trim(),
            BranchCode = normalizedCode,
            Address = req.Address.Trim(),
            Phone = req.Phone.Trim(),
            Latitude = req.Latitude,
            Longitude = req.Longitude,
            IsActive = true
        };

        _context.Branches.Add(branch);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<BranchDto>.Success(new BranchDto(
            branch.Id,
            branch.RestaurantId,
            branch.BranchName,
            branch.BranchCode,
            branch.Address,
            branch.Phone,
            branch.Latitude,
            branch.Longitude,
            branch.IsActive
        ));
    }
}

public record GetBranchesQuery() : IRequest<Result<List<BranchDto>>>;
public class GetBranchesQueryHandler : IRequestHandler<GetBranchesQuery, Result<List<BranchDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetBranchesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<BranchDto>>> Handle(GetBranchesQuery request, CancellationToken cancellationToken)
    {
        var branches = await _context.Branches
            .OrderBy(b => b.BranchName)
            .Select(b => new BranchDto(b.Id, b.RestaurantId, b.BranchName, b.BranchCode, b.Address, b.Phone, b.Latitude, b.Longitude, b.IsActive))
            .ToListAsync(cancellationToken);

        return Result<List<BranchDto>>.Success(branches);
    }
}
