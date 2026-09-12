using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using RestaurantSaaS.Application.Interfaces;
using RestaurantSaaS.Domain.Entities;
using RestaurantSaaS.Domain.Enums;

namespace RestaurantSaaS.Application.Taxation.Commands;

public record ConfigureTaxRuleCommand(
    Guid RestaurantId,
    Guid? BranchId,
    string Name,
    decimal RatePercentage,
    OrderType? AppliesToOrderType,
    PaymentMethod? AppliesToPaymentMethod,
    string? Description
) : IRequest<Guid>;

public class ConfigureTaxRuleCommandValidator : AbstractValidator<ConfigureTaxRuleCommand>
{
    public ConfigureTaxRuleCommandValidator()
    {
        RuleFor(x => x.RestaurantId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.RatePercentage).InclusiveBetween(0.00m, 100.00m);
    }
}

public class ConfigureTaxRuleCommandHandler : IRequestHandler<ConfigureTaxRuleCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public ConfigureTaxRuleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(ConfigureTaxRuleCommand request, CancellationToken cancellationToken)
    {
        var rule = new TaxRule
        {
            RestaurantId = request.RestaurantId,
            BranchId = request.BranchId,
            Name = request.Name,
            RatePercentage = request.RatePercentage,
            AppliesToOrderType = request.AppliesToOrderType,
            AppliesToPaymentMethod = request.AppliesToPaymentMethod,
            Description = request.Description,
            IsActive = true
        };

        _context.TaxRules.Add(rule);
        await _context.SaveChangesAsync(cancellationToken);

        return rule.Id;
    }
}
