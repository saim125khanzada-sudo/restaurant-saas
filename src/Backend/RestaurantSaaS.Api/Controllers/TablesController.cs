using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantSaaS.Application.DTOs;
using RestaurantSaaS.Application.Features.Tables.Commands;

namespace RestaurantSaaS.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class TablesController : ControllerBase
{
    private readonly IMediator _mediator;

    public TablesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetTables([FromQuery] Guid? branchId)
    {
        var result = await _mediator.Send(new GetTablesQuery(branchId));
        return Ok(result.Value);
    }

    [HttpPost]
    [Authorize(Policy = "Branches.Manage")]
    public async Task<IActionResult> CreateTable([FromBody] CreateTableRequest request)
    {
        var result = await _mediator.Send(new CreateTableCommand(request));
        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(GetTables), new { id = result.Value!.Id }, result.Value);
    }

    [HttpPost("floor-sections")]
    [Authorize(Policy = "Branches.Manage")]
    public async Task<IActionResult> CreateFloorSection([FromBody] CreateFloorSectionRequest request)
    {
        var result = await _mediator.Send(new CreateFloorSectionCommand(request));
        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }
}
