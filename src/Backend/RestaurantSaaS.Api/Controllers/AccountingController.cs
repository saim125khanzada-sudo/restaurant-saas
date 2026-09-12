using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantSaaS.Application.Accounting.Commands;
using RestaurantSaaS.Application.Accounting.Queries;

namespace RestaurantSaaS.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class AccountingController : ControllerBase
{
    private readonly IMediator _mediator;

    public AccountingController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("registers/open")]
    public async Task<ActionResult<Guid>> OpenSession([FromBody] OpenCashRegisterCommand command)
    {
        var id = await _mediator.Send(command);
        return Ok(new { sessionId = id });
    }

    [HttpPost("registers/close")]
    public async Task<ActionResult> CloseSession([FromBody] CloseCashRegisterCommand command)
    {
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpGet("trial-balance")]
    public async Task<ActionResult<TrialBalanceReportDto>> GetTrialBalance([FromQuery] DateTime? asOfDate)
    {
        var report = await _mediator.Send(new GetTrialBalanceQuery(asOfDate));
        return Ok(report);
    }
}
