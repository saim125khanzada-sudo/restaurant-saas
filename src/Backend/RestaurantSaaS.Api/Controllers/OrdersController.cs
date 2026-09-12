using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantSaaS.Application.DTOs;
using RestaurantSaaS.Application.Features.Orders.Commands;
using RestaurantSaaS.Application.Features.Orders.Queries;
using RestaurantSaaS.Domain.Enums;

namespace RestaurantSaaS.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize(Policy = "Orders.View")]
    public async Task<IActionResult> GetOrders([FromQuery] Guid? branchId, [FromQuery] OrderStatus? status, [FromQuery] DateTimeOffset? date)
    {
        var result = await _mediator.Send(new GetOrdersQuery(branchId, status, date));
        return Ok(result.Value);
    }

    [HttpPost]
    [Authorize(Policy = "Orders.Create")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var result = await _mediator.Send(new CreateOrderCommand(request));
        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpPost("transition-status")]
    [Authorize(Policy = "Kitchen.StatusUpdate")]
    public async Task<IActionResult> TransitionStatus([FromBody] TransitionOrderStatusRequest request)
    {
        var result = await _mediator.Send(new TransitionOrderStatusCommand(request));
        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(new { status = result.Value.ToString() });
    }

    [HttpGet("{id}/receipt")]
    [Authorize(Policy = "Orders.View")]
    public async Task<IActionResult> GetReceipt(Guid id)
    {
        var result = await _mediator.Send(new GenerateReceiptQuery(id));
        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }
}
