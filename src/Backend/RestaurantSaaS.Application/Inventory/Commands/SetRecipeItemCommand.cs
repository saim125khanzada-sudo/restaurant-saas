using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using RestaurantSaaS.Application.Interfaces;
using RestaurantSaaS.Domain.Entities;

namespace RestaurantSaaS.Application.Inventory.Commands;

public record SetRecipeItemCommand(
    Guid ProductId,
    Guid? ProductVariantId,
    Guid IngredientId,
    decimal QuantityRequired
) : IRequest<Guid>;

public class SetRecipeItemCommandValidator : AbstractValidator<SetRecipeItemCommand>
{
    public SetRecipeItemCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.IngredientId).NotEmpty();
        RuleFor(x => x.QuantityRequired).GreaterThan(0);
    }
}

public class SetRecipeItemCommandHandler : IRequestHandler<SetRecipeItemCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenantService _tenantService;

    public SetRecipeItemCommandHandler(IApplicationDbContext context, ICurrentTenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    public async Task<Guid> Handle(SetRecipeItemCommand request, CancellationToken cancellationToken)
    {
        var recipe = new RecipeItem
        {
            RestaurantId = _tenantService.RestaurantId ?? Guid.Empty,
            ProductId = request.ProductId,
            ProductVariantId = request.ProductVariantId,
            IngredientId = request.IngredientId,
            QuantityRequired = request.QuantityRequired
        };

        _context.RecipeItems.Add(recipe);
        await _context.SaveChangesAsync(cancellationToken);

        return recipe.Id;
    }
}
