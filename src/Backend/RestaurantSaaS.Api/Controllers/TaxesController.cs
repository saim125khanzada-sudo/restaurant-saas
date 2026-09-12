using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantSaaS.Application.Taxation.Commands;
using RestaurantSaaS.Application.Taxation.Queries;

namespace RestaurantSaaS.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class TaxesController : ControllerBase
{
    private readonly IMediator _mediator;

    public TaxesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("rules")]
    public async Task<ActionResult<Guid>> ConfigureTaxRule([FromBody] ConfigureTaxRuleCommand command)
    {
        var ruleId = await _mediator.Send(command);
        return CreatedAtAction(nameof(ConfigureTaxRule), new { id = ruleId }, new { ruleId });
    }

    [HttpGet("restaurants/{restaurantId}/rules")]
    public async Task<ActionResult<List<TaxRuleDto>>> GetTaxRules(Guid restaurantId, [FromQuery] Guid? branchId)
    {
        var rules = await _mediator.Send(new GetTaxRulesQuery(restaurantId, branchId));
        return Ok(rules);
    }

    [HttpPost("fiscalize/order/{orderId}")]
    public async Task<ActionResult<FiscalInvoiceResultDto>> FiscalizeOrder(Guid orderId)
    {
        var result = await _mediator.Send(new TransmitFiscalInvoiceCommand(orderId));
        return Ok(result);
    }

    [HttpGet("restaurants/{restaurantId}/summary")]
    public async Task<ActionResult<TaxSummaryReportDto>> GetTaxSummary(Guid restaurantId, [FromQuery] Guid? branchId)
    {
        var summary = await _mediator.Send(new GetTaxSummaryQuery(restaurantId, branchId));
        return Ok(summary);
    }
}
