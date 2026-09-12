using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using RestaurantSaaS.Application.Interfaces;
using RestaurantSaaS.Domain.Entities;

namespace RestaurantSaaS.Application.Inventory.Commands;

public record CreateIngredientCommand(
    string Name,
    string UnitOfMeasure,
    decimal ReorderThreshold,
    decimal CostPerUnit
) : IRequest<Guid>;

public class CreateIngredientCommandValidator : AbstractValidator<CreateIngredientCommand>
{
    public CreateIngredientCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.UnitOfMeasure).NotEmpty().MaximumLength(20);
        RuleFor(x => x.ReorderThreshold).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CostPerUnit).GreaterThanOrEqualTo(0);
    }
}

public class CreateIngredientCommandHandler : IRequestHandler<CreateIngredientCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenantService _tenantService;

    public CreateIngredientCommandHandler(IApplicationDbContext context, ICurrentTenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    public async Task<Guid> Handle(CreateIngredientCommand request, CancellationToken cancellationToken)
    {
        var ingredient = new Ingredient
        {
            RestaurantId = _tenantService.RestaurantId ?? Guid.Empty,
            Name = request.Name,
            UnitOfMeasure = request.UnitOfMeasure,
            ReorderThreshold = request.ReorderThreshold,
            CostPerUnit = request.CostPerUnit
        };

        _context.Ingredients.Add(ingredient);
        await _context.SaveChangesAsync(cancellationToken);

        return ingredient.Id;
    }
}
