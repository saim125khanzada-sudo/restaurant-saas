using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantSaaS.Application.Interfaces;
using RestaurantSaaS.Domain.Entities;
using RestaurantSaaS.Domain.Enums;

namespace RestaurantSaaS.Application.Inventory.Commands;

public record RecordStockAdjustmentCommand(
    Guid BranchId,
    Guid IngredientId,
    decimal AdjustedQuantity, // +/- change
    StockMovementType MovementType,
    string? Reason
) : IRequest<bool>;

public class RecordStockAdjustmentCommandValidator : AbstractValidator<RecordStockAdjustmentCommand>
{
    public RecordStockAdjustmentCommandValidator()
    {
        RuleFor(x => x.BranchId).NotEmpty();
        RuleFor(x => x.IngredientId).NotEmpty();
        RuleFor(x => x.MovementType).IsInEnum();
    }
}

public class RecordStockAdjustmentCommandHandler : IRequestHandler<RecordStockAdjustmentCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenantService _tenantService;

    public RecordStockAdjustmentCommandHandler(IApplicationDbContext context, ICurrentTenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    public async Task<bool> Handle(RecordStockAdjustmentCommand request, CancellationToken cancellationToken)
    {
        var stockLevel = await _context.StockLevels
            .FirstOrDefaultAsync(s => s.BranchId == request.BranchId && s.IngredientId == request.IngredientId, cancellationToken);

        if (stockLevel == null)
        {
            stockLevel = new StockLevel
            {
                RestaurantId = _tenantService.RestaurantId ?? Guid.Empty,
                BranchId = request.BranchId,
                IngredientId = request.IngredientId,
                QuantityOnHand = 0
            };
            _context.StockLevels.Add(stockLevel);
        }

        stockLevel.QuantityOnHand += request.AdjustedQuantity;

        var movement = new StockMovement
        {
            RestaurantId = _tenantService.RestaurantId ?? Guid.Empty,
            BranchId = request.BranchId,
            IngredientId = request.IngredientId,
            MovementType = request.MovementType,
            Quantity = request.AdjustedQuantity,
            Reason = request.Reason
        };

        _context.StockMovements.Add(movement);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
