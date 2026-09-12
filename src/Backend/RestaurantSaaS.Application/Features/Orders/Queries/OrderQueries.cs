using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantSaaS.Application.DTOs;
using RestaurantSaaS.Application.Interfaces;
using RestaurantSaaS.Domain.Enums;
using RestaurantSaaS.SharedKernel.Common;

namespace RestaurantSaaS.Application.Features.Orders.Queries;

public record GetOrdersQuery(Guid? BranchId, OrderStatus? Status, DateTimeOffset? Date) : IRequest<Result<List<OrderDto>>>;

public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, Result<List<OrderDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenantService _tenantService;

    public GetOrdersQueryHandler(IApplicationDbContext context, ICurrentTenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    public async Task<Result<List<OrderDto>>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var branchId = request.BranchId ?? _tenantService.BranchId;
        var query = _context.Orders
            .Include(o => o.Table)
            .Include(o => o.Waiter)
            .Include(o => o.Items)
                .ThenInclude(i => i.Addons)
            .AsQueryable();

        if (branchId.HasValue)
            query = query.Where(o => o.BranchId == branchId.Value);

        if (request.Status.HasValue)
            query = query.Where(o => o.Status == request.Status.Value);

        if (request.Date.HasValue)
        {
            var targetDate = request.Date.Value.Date;
            query = query.Where(o => o.CreatedAt.Date == targetDate);
        }

        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new OrderDto(
                o.Id,
                o.RestaurantId,
                o.BranchId,
                o.OrderNumber,
                o.OrderType,
                o.Status,
                o.PaymentStatus,
                o.TableId,
                o.Table != null ? o.Table.TableNumber : null,
                o.WaiterId,
                o.Waiter != null ? o.Waiter.FullName : null,
                o.CustomerName,
                o.CustomerPhone,
                o.Subtotal,
                o.TaxTotal,
                o.DiscountTotal,
                o.GrandTotal,
                o.CreatedAt,
                o.Items.Select(i => new OrderItemDto(
                    i.Id,
                    i.ProductId,
                    i.ItemName,
                    i.Quantity,
                    i.UnitPrice,
                    i.TotalPrice,
                    i.KitchenNotes,
                    i.Addons.Select(a => new OrderItemAddonRequest(a.AddonId, a.AddonName, a.UnitPrice, a.Quantity)).ToList()
                )).ToList()
            ))
            .ToListAsync(cancellationToken);

        return Result<List<OrderDto>>.Success(orders);
    }
}

public record GenerateReceiptQuery(Guid OrderId) : IRequest<Result<ReceiptPrintDto>>;
public class GenerateReceiptQueryHandler : IRequestHandler<GenerateReceiptQuery, Result<ReceiptPrintDto>>
{
    private readonly IApplicationDbContext _context;

    public GenerateReceiptQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ReceiptPrintDto>> Handle(GenerateReceiptQuery request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .Include(o => o.Branch)
            .Include(o => o.Table)
            .Include(o => o.Items)
                .ThenInclude(i => i.Addons)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order == null)
            return Result<ReceiptPrintDto>.Failure("Order not found.");

        var restaurant = await _context.Restaurants.FindAsync(new object[] { order.RestaurantId }, cancellationToken);
        var lines = new List<string>();

        foreach (var item in order.Items)
        {
            lines.Add($"{item.Quantity}x {item.ItemName} - {item.TotalPrice:C2}");
            foreach (var addon in item.Addons)
            {
                lines.Add($"   + {addon.Quantity}x {addon.AddonName} ({addon.TotalPrice:C2})");
            }
        }

        var receipt = new ReceiptPrintDto(
            OrderNumber: order.OrderNumber,
            RestaurantName: restaurant?.Name ?? "Restaurant SaaS",
            BranchName: order.Branch?.BranchName ?? "Main Branch",
            DateFormatted: order.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
            OrderType: order.OrderType.ToString(),
            TableNumber: order.Table?.TableNumber,
            Lines: lines,
            Subtotal: order.Subtotal,
            Tax: order.TaxTotal,
            Total: order.GrandTotal,
            FooterMessage: "Thank you for dining with us! Please visit again."
        );

        return Result<ReceiptPrintDto>.Success(receipt);
    }
}
