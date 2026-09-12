using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantSaaS.Application.DTOs;
using RestaurantSaaS.Application.Features.Branches.Commands;

namespace RestaurantSaaS.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class BranchesController : ControllerBase
{
    private readonly IMediator _mediator;

    public BranchesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetBranches()
    {
        var result = await _mediator.Send(new GetBranchesQuery());
        return Ok(result.Value);
    }

    [HttpPost]
    [Authorize(Policy = "Branches.Manage")]
    public async Task<IActionResult> CreateBranch([FromBody] CreateBranchRequest request)
    {
        var result = await _mediator.Send(new CreateBranchCommand(request));
        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(GetBranches), new { id = result.Value!.Id }, result.Value);
    }
}
