using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantSaaS.Application.Interfaces;
using RestaurantSaaS.Domain.Enums;

namespace RestaurantSaaS.Application.Reporting.Queries;

// 1. Sales Report
public record SalesReportDto(
    DateTime FromDate,
    DateTime ToDate,
    int TotalOrders,
    decimal GrossSales,
    decimal TotalDiscounts,
    decimal TotalTaxes,
    decimal NetRevenue,
    Dictionary<string, int> OrdersByType,
    Dictionary<string, decimal> RevenueByPaymentMethod
);

public record GetSalesReportQuery(
    Guid RestaurantId,
    Guid? BranchId,
    DateTime FromDate,
    DateTime ToDate
) : IRequest<SalesReportDto>;

public class GetSalesReportQueryHandler : IRequestHandler<GetSalesReportQuery, SalesReportDto>
{
    private readonly IApplicationDbContext _context;

    public GetSalesReportQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SalesReportDto> Handle(GetSalesReportQuery request, CancellationToken cancellationToken)
    {
        var fromUtc = DateTime.SpecifyKind(request.FromDate, DateTimeKind.Utc);
        var toUtc = DateTime.SpecifyKind(request.ToDate, DateTimeKind.Utc);

        var query = _context.Orders
            .Include(o => o.Payments)
            .Where(o => o.RestaurantId == request.RestaurantId &&
                        o.CreatedAt >= fromUtc &&
                        o.CreatedAt <= toUtc &&
                        o.Status == OrderStatus.Completed);

        if (request.BranchId.HasValue)
        {
            query = query.Where(o => o.BranchId == request.BranchId.Value);
        }

        var orders = await query.ToListAsync(cancellationToken);

        var totalOrders = orders.Count;
        var grossSales = orders.Sum(o => o.Subtotal);
        var totalDiscounts = orders.Sum(o => o.DiscountTotal);
        var totalTaxes = orders.Sum(o => o.TaxTotal);
        var netRevenue = orders.Sum(o => o.GrandTotal);

        var ordersByType = orders
            .GroupBy(o => o.OrderType.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        var revenueByPayment = orders
            .SelectMany(o => o.Payments)
            .GroupBy(p => p.Method.ToString())
            .ToDictionary(g => g.Key, g => g.Sum(p => p.Amount));

        return new SalesReportDto(
            request.FromDate,
            request.ToDate,
            totalOrders,
            grossSales,
            totalDiscounts,
            totalTaxes,
            netRevenue,
            ordersByType,
            revenueByPayment
        );
    }
}

// 2. Hourly Rush Analytics
public record HourlyRushDto(
    int HourOfDay, // 0 - 23
    int OrderCount,
    decimal Revenue
);

public record GetHourlyRushAnalyticsQuery(
    Guid RestaurantId,
    Guid? BranchId,
    DateTime Date
) : IRequest<List<HourlyRushDto>>;

public class GetHourlyRushAnalyticsQueryHandler : IRequestHandler<GetHourlyRushAnalyticsQuery, List<HourlyRushDto>>
{
    private readonly IApplicationDbContext _context;

    public GetHourlyRushAnalyticsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<HourlyRushDto>> Handle(GetHourlyRushAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var startUtc = DateTime.SpecifyKind(request.Date.Date, DateTimeKind.Utc);
        var endUtc = startUtc.AddDays(1);

        var query = _context.Orders
            .Where(o => o.RestaurantId == request.RestaurantId &&
                        o.CreatedAt >= startUtc &&
                        o.CreatedAt < endUtc);

        if (request.BranchId.HasValue)
        {
            query = query.Where(o => o.BranchId == request.BranchId.Value);
        }

        var orders = await query.ToListAsync(cancellationToken);

        var result = new List<HourlyRushDto>();
        for (int h = 0; h < 24; h++)
        {
            var bucket = orders.Where(o => o.CreatedAt.Hour == h).ToList();
            result.Add(new HourlyRushDto(
                HourOfDay: h,
                OrderCount: bucket.Count,
                Revenue: bucket.Sum(b => b.GrandTotal)
            ));
        }

        return result;
    }
}

// 3. Top Selling Products
public record TopProductDto(
    string ProductName,
    int QuantitySold,
    decimal TotalSales
);

public record GetTopSellingProductsQuery(
    Guid RestaurantId,
    Guid? BranchId,
    int Limit = 10
) : IRequest<List<TopProductDto>>;

public class GetTopSellingProductsQueryHandler : IRequestHandler<GetTopSellingProductsQuery, List<TopProductDto>>
{
    private readonly IApplicationDbContext _context;

    public GetTopSellingProductsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TopProductDto>> Handle(GetTopSellingProductsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.OrderItems
            .Include(oi => oi.Order)
            .Where(oi => oi.RestaurantId == request.RestaurantId &&
                         oi.Order.Status == OrderStatus.Completed);

        if (request.BranchId.HasValue)
        {
            query = query.Where(oi => oi.Order.BranchId == request.BranchId.Value);
        }

        var items = await query.ToListAsync(cancellationToken);

        return items
            .GroupBy(i => i.ItemName)
            .Select(g => new TopProductDto(
                ProductName: g.Key,
                QuantitySold: g.Sum(x => x.Quantity),
                TotalSales: g.Sum(x => x.TotalPrice)
            ))
            .OrderByDescending(x => x.QuantitySold)
            .Take(request.Limit)
            .ToList();
    }
}
