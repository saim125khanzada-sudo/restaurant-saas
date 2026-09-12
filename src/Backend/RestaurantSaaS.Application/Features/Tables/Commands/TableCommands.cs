using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantSaaS.Application.DTOs;
using RestaurantSaaS.Application.Interfaces;
using RestaurantSaaS.Domain.Entities;
using RestaurantSaaS.Domain.Enums;
using RestaurantSaaS.SharedKernel.Common;

namespace RestaurantSaaS.Application.Features.Tables.Commands;

public record CreateFloorSectionCommand(CreateFloorSectionRequest Request) : IRequest<Result<FloorSectionDto>>;
public class CreateFloorSectionCommandHandler : IRequestHandler<CreateFloorSectionCommand, Result<FloorSectionDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenantService _tenantService;

    public CreateFloorSectionCommandHandler(IApplicationDbContext context, ICurrentTenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    public async Task<Result<FloorSectionDto>> Handle(CreateFloorSectionCommand command, CancellationToken cancellationToken)
    {
        if (!_tenantService.RestaurantId.HasValue)
            return Result<FloorSectionDto>.Failure("Tenant context is required.");

        var req = command.Request;
        var section = new FloorSection
        {
            RestaurantId = _tenantService.RestaurantId.Value,
            BranchId = req.BranchId ?? _tenantService.BranchId,
            Name = req.Name.Trim(),
            Description = req.Description?.Trim(),
            DisplayOrder = req.DisplayOrder
        };

        _context.FloorSections.Add(section);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<FloorSectionDto>.Success(new FloorSectionDto(
            section.Id,
            section.BranchId,
            section.Name,
            section.Description,
            section.DisplayOrder
        ));
    }
}

public record CreateTableCommand(CreateTableRequest Request) : IRequest<Result<TableDto>>;
public class CreateTableCommandHandler : IRequestHandler<CreateTableCommand, Result<TableDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenantService _tenantService;

    public CreateTableCommandHandler(IApplicationDbContext context, ICurrentTenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    public async Task<Result<TableDto>> Handle(CreateTableCommand command, CancellationToken cancellationToken)
    {
        if (!_tenantService.RestaurantId.HasValue)
            return Result<TableDto>.Failure("Tenant context is required.");

        var req = command.Request;
        var branchId = req.BranchId ?? _tenantService.BranchId;
        var normalizedNumber = req.TableNumber.Trim().ToUpperInvariant();

        if (await _context.RestaurantTables.AnyAsync(t => t.BranchId == branchId && t.TableNumber == normalizedNumber, cancellationToken))
        {
            return Result<TableDto>.Failure($"Table '{req.TableNumber}' already exists in this branch.");
        }

        var table = new RestaurantTable
        {
            RestaurantId = _tenantService.RestaurantId.Value,
            BranchId = branchId,
            FloorSectionId = req.FloorSectionId,
            TableNumber = normalizedNumber,
            Capacity = req.Capacity,
            Status = TableStatus.Available
        };

        _context.RestaurantTables.Add(table);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<TableDto>.Success(new TableDto(
            table.Id,
            table.BranchId,
            table.FloorSectionId,
            table.TableNumber,
            table.Capacity,
            table.Status,
            table.CurrentOrderId
        ));
    }
}

public record GetTablesQuery(Guid? BranchId) : IRequest<Result<List<TableDto>>>;
public class GetTablesQueryHandler : IRequestHandler<GetTablesQuery, Result<List<TableDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenantService _tenantService;

    public GetTablesQueryHandler(IApplicationDbContext context, ICurrentTenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    public async Task<Result<List<TableDto>>> Handle(GetTablesQuery request, CancellationToken cancellationToken)
    {
        var branchId = request.BranchId ?? _tenantService.BranchId;
        var query = _context.RestaurantTables.AsQueryable();

        if (branchId.HasValue)
        {
            query = query.Where(t => t.BranchId == branchId.Value);
        }

        var tables = await query
            .OrderBy(t => t.TableNumber)
            .Select(t => new TableDto(t.Id, t.BranchId, t.FloorSectionId, t.TableNumber, t.Capacity, t.Status, t.CurrentOrderId))
            .ToListAsync(cancellationToken);

        return Result<List<TableDto>>.Success(tables);
    }
}
