using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantSaaS.Application.Interfaces;
using RestaurantSaaS.Domain.Enums;
using RestaurantSaaS.Domain.Exceptions;

namespace RestaurantSaaS.Application.Accounting.Commands;

public record CloseCashRegisterCommand(
    Guid SessionId,
    decimal ActualCountedCash,
    string? ClosingNotes
) : IRequest<bool>;

public class CloseCashRegisterCommandValidator : AbstractValidator<CloseCashRegisterCommand>
{
    public CloseCashRegisterCommandValidator()
    {
        RuleFor(x => x.SessionId).NotEmpty();
        RuleFor(x => x.ActualCountedCash).GreaterThanOrEqualTo(0);
    }
}

public class CloseCashRegisterCommandHandler : IRequestHandler<CloseCashRegisterCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public CloseCashRegisterCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(CloseCashRegisterCommand request, CancellationToken cancellationToken)
    {
        var session = await _context.CashRegisterSessions
            .FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken)
            ?? throw new NotFoundException($"Cash register session {request.SessionId} not found.");

        session.ClosedAt = DateTime.UtcNow;
        session.ActualCountedCash = request.ActualCountedCash;
        session.Discrepancy = request.ActualCountedCash - session.ExpectedCash;
        session.Status = CashRegisterSessionStatus.Closed;
        if (!string.IsNullOrWhiteSpace(request.ClosingNotes))
        {
            session.Notes = string.IsNullOrWhiteSpace(session.Notes)
                ? request.ClosingNotes
                : $"{session.Notes} | Closing: {request.ClosingNotes}";
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
