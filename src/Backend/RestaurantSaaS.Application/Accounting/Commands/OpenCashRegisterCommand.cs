using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using RestaurantSaaS.Application.Interfaces;
using RestaurantSaaS.Domain.Entities;
using RestaurantSaaS.Domain.Enums;

namespace RestaurantSaaS.Application.Accounting.Commands;

public record OpenCashRegisterCommand(
    Guid BranchId,
    decimal OpeningFloat,
    string? Notes
) : IRequest<Guid>;

public class OpenCashRegisterCommandValidator : AbstractValidator<OpenCashRegisterCommand>
{
    public OpenCashRegisterCommandValidator()
    {
        RuleFor(x => x.BranchId).NotEmpty();
        RuleFor(x => x.OpeningFloat).GreaterThanOrEqualTo(0);
    }
}

public class OpenCashRegisterCommandHandler : IRequestHandler<OpenCashRegisterCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenantService _tenantService;

    public OpenCashRegisterCommandHandler(IApplicationDbContext context, ICurrentTenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    public async Task<Guid> Handle(OpenCashRegisterCommand request, CancellationToken cancellationToken)
    {
        var session = new CashRegisterSession
        {
            RestaurantId = _tenantService.RestaurantId ?? Guid.Empty,
            BranchId = request.BranchId,
            CashierUserId = _tenantService.UserId ?? Guid.Empty,
            OpenedAt = DateTime.UtcNow,
            OpeningFloat = request.OpeningFloat,
            ExpectedCash = request.OpeningFloat,
            Status = CashRegisterSessionStatus.Open,
            Notes = request.Notes
        };

        _context.CashRegisterSessions.Add(session);
        await _context.SaveChangesAsync(cancellationToken);

        return session.Id;
    }
}
