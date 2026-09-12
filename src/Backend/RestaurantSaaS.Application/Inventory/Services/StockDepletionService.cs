using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RestaurantSaaS.Application.Interfaces;
using RestaurantSaaS.Domain.Entities;
using RestaurantSaaS.Domain.Enums;

namespace RestaurantSaaS.Application.Inventory.Services;

public interface IStockDepletionService
{
    Task DepleteStockForOrderAsync(Guid orderId, CancellationToken cancellationToken = default);
}

public class StockDepletionService : IStockDepletionService
{
    private readonly IApplicationDbContext _context;

    public StockDepletionService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task DepleteStockForOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order == null || !order.BranchId.HasValue) return;

        var branchId = order.BranchId.Value;

        foreach (var orderItem in order.Items)
        {
            // Find recipes for this product / variant
            var recipeItems = await _context.RecipeItems
                .Where(r => r.ProductId == orderItem.ProductId &&
                           (r.ProductVariantId == null || r.ProductVariantId == orderItem.ProductVariantId))
                .ToListAsync(cancellationToken);

            foreach (var recipe in recipeItems)
            {
                var totalDeduction = recipe.QuantityRequired * orderItem.Quantity;

                // Find or create branch stock level
                var stockLevel = await _context.StockLevels
                    .FirstOrDefaultAsync(s => s.BranchId == branchId && s.IngredientId == recipe.IngredientId, cancellationToken);

                if (stockLevel == null)
                {
                    stockLevel = new StockLevel
                    {
                        RestaurantId = order.RestaurantId,
                        BranchId = branchId,
                        IngredientId = recipe.IngredientId,
                        QuantityOnHand = 0
                    };
                    _context.StockLevels.Add(stockLevel);
                }

                // Decrement stock
                stockLevel.QuantityOnHand -= totalDeduction;

                // Create immutable StockMovement audit record
                var movement = new StockMovement
                {
                    RestaurantId = order.RestaurantId,
                    BranchId = branchId,
                    IngredientId = recipe.IngredientId,
                    MovementType = StockMovementType.OrderConsumption,
                    Quantity = -totalDeduction,
                    UnitCost = 0, // Computed or historical
                    ReferenceOrderId = order.Id,
                    Reason = $"Order #{order.OrderNumber} consumption"
                };

                _context.StockMovements.Add(movement);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
