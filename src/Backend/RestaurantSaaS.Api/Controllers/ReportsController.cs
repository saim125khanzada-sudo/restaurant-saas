using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantSaaS.Application.Reporting.Queries;

namespace RestaurantSaaS.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReportsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("restaurants/{restaurantId}/sales")]
    public async Task<ActionResult<SalesReportDto>> GetSalesReport(
        Guid restaurantId,
        [FromQuery] Guid? branchId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate)
    {
        var from = fromDate ?? DateTime.UtcNow.AddDays(-30);
        var to = toDate ?? DateTime.UtcNow;

        var report = await _mediator.Send(new GetSalesReportQuery(restaurantId, branchId, from, to));
        return Ok(report);
    }

    [HttpGet("restaurants/{restaurantId}/rush-hours")]
    public async Task<ActionResult<List<HourlyRushDto>>> GetRushHours(
        Guid restaurantId,
        [FromQuery] Guid? branchId,
        [FromQuery] DateTime? date)
    {
        var targetDate = date ?? DateTime.UtcNow;
        var list = await _mediator.Send(new GetHourlyRushAnalyticsQuery(restaurantId, branchId, targetDate));
        return Ok(list);
    }

    [HttpGet("restaurants/{restaurantId}/top-products")]
    public async Task<ActionResult<List<TopProductDto>>> GetTopProducts(
        Guid restaurantId,
        [FromQuery] Guid? branchId,
        [FromQuery] int limit = 10)
    {
        var list = await _mediator.Send(new GetTopSellingProductsQuery(restaurantId, branchId, limit));
        return Ok(list);
    }
}
