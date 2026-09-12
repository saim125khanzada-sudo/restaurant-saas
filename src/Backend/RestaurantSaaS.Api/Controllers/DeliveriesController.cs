using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantSaaS.Application.Deliveries.Commands;
using RestaurantSaaS.Application.Deliveries.Queries;

namespace RestaurantSaaS.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class DeliveriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public DeliveriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("assign")]
    public async Task<ActionResult<Guid>> AssignDelivery([FromBody] AssignDeliveryCommand command)
    {
        var dispatchId = await _mediator.Send(command);
        return Ok(new { dispatchId });
    }

    [HttpPatch("{id}/status")]
    public async Task<ActionResult> UpdateStatus(Guid id, [FromBody] UpdateDeliveryStatusCommand command)
    {
        if (id != command.DispatchId)
        {
            return BadRequest("ID mismatch in URL and body.");
        }
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpGet("rider/{riderId}/active")]
    public async Task<ActionResult<List<DeliveryDispatchDto>>> GetRiderActive(Guid riderId)
    {
        var result = await _mediator.Send(new GetRiderActiveDeliveriesQuery(riderId));
        return Ok(result);
    }

    [HttpPost("reconcile")]
    public async Task<ActionResult<Guid>> ReconcileCash([FromBody] ReconcileRiderCashCommand command)
    {
        var reconId = await _mediator.Send(command);
        return Ok(new { reconciliationId = reconId });
    }
}
