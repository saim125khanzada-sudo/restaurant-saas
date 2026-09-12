using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantSaaS.Application.Subscriptions.Commands;
using RestaurantSaaS.Application.Subscriptions.Queries;

namespace RestaurantSaaS.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class SubscriptionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SubscriptionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("plans")]
    public async Task<ActionResult<Guid>> CreatePlan([FromBody] CreateSubscriptionPlanCommand command)
    {
        var planId = await _mediator.Send(command);
        return CreatedAtAction(nameof(CreatePlan), new { id = planId }, new { planId });
    }

    [HttpGet("plans")]
    public async Task<ActionResult<List<SubscriptionPlanDto>>> GetPlans()
    {
        var plans = await _mediator.Send(new GetSubscriptionPlansQuery());
        return Ok(plans);
    }

    [HttpPost("subscribe")]
    public async Task<ActionResult<Guid>> SubscribeTenant([FromBody] SubscribeTenantCommand command)
    {
        var subId = await _mediator.Send(command);
        return Ok(new { subscriptionId = subId });
    }

    [HttpGet("restaurants/{restaurantId}/status")]
    public async Task<ActionResult<TenantSubscriptionStatusDto>> GetStatus(Guid restaurantId)
    {
        var status = await _mediator.Send(new GetTenantSubscriptionStatusQuery(restaurantId));
        return Ok(status);
    }
}
