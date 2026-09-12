using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantSaaS.Application.Inventory.Commands;
using RestaurantSaaS.Application.Inventory.Queries;

namespace RestaurantSaaS.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class InventoryController : ControllerBase
{
    private readonly IMediator _mediator;

    public InventoryController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("ingredients")]
    public async Task<ActionResult<Guid>> CreateIngredient([FromBody] CreateIngredientCommand command)
    {
        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(CreateIngredient), new { id }, new { id });
    }

    [HttpPost("recipes")]
    public async Task<ActionResult<Guid>> SetRecipe([FromBody] SetRecipeItemCommand command)
    {
        var id = await _mediator.Send(command);
        return Ok(new { recipeItemId = id });
    }

    [HttpPost("adjustments")]
    public async Task<ActionResult> AdjustStock([FromBody] RecordStockAdjustmentCommand command)
    {
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpGet("branches/{branchId}/stock")]
    public async Task<ActionResult<List<IngredientStockDto>>> GetStockLevels(Guid branchId)
    {
        var levels = await _mediator.Send(new GetBranchStockLevelsQuery(branchId));
        return Ok(levels);
    }
}
