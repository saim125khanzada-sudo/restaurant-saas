using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantSaaS.Application.Interfaces;

namespace RestaurantSaaS.Application.Inventory.Queries;

public record IngredientStockDto(
    Guid IngredientId,
    string Name,
    string UnitOfMeasure,
    decimal QuantityOnHand,
    decimal ReorderThreshold,
    bool IsLowStock
);

public record GetBranchStockLevelsQuery(Guid BranchId) : IRequest<List<IngredientStockDto>>;

public class GetBranchStockLevelsQueryHandler : IRequestHandler<GetBranchStockLevelsQuery, List<IngredientStockDto>>
{
    private readonly IApplicationDbContext _context;

    public GetBranchStockLevelsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<IngredientStockDto>> Handle(GetBranchStockLevelsQuery request, CancellationToken cancellationToken)
    {
        var ingredients = await _context.Ingredients.Where(i => i.IsActive).ToListAsync(cancellationToken);
        var stockLevels = await _context.StockLevels.Where(s => s.BranchId == request.BranchId).ToListAsync(cancellationToken);

        var result = new List<IngredientStockDto>();
        foreach (var ing in ingredients)
        {
            var level = stockLevels.FirstOrDefault(s => s.IngredientId == ing.Id)?.QuantityOnHand ?? 0;
            result.Add(new IngredientStockDto(
                ing.Id,
                ing.Name,
                ing.UnitOfMeasure,
                level,
                ing.ReorderThreshold,
                level <= ing.ReorderThreshold
            ));
        }

        return result;
    }
}
