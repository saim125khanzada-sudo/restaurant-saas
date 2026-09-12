using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantSaaS.Application.Interfaces;
using RestaurantSaaS.Domain.Enums;

namespace RestaurantSaaS.Application.Accounting.Queries;

public record TrialBalanceRowDto(
    string AccountCode,
    string AccountName,
    AccountClassification Classification,
    decimal TotalDebit,
    decimal TotalCredit,
    decimal NetBalance
);

public record TrialBalanceReportDto(
    List<TrialBalanceRowDto> Rows,
    decimal TotalDebits,
    decimal TotalCredits,
    bool IsBalanced
);

public record GetTrialBalanceQuery(DateTime? AsOfDate) : IRequest<TrialBalanceReportDto>;

public class GetTrialBalanceQueryHandler : IRequestHandler<GetTrialBalanceQuery, TrialBalanceReportDto>
{
    private readonly IApplicationDbContext _context;

    public GetTrialBalanceQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TrialBalanceReportDto> Handle(GetTrialBalanceQuery request, CancellationToken cancellationToken)
    {
        var cutoff = request.AsOfDate ?? DateTime.UtcNow;

        var accounts = await _context.Accounts
            .Where(a => a.IsActive)
            .OrderBy(a => a.AccountCode)
            .ToListAsync(cancellationToken);

        var lines = await _context.JournalLines
            .Include(l => l.Account)
            .Where(l => l.CreatedAt <= cutoff)
            .ToListAsync(cancellationToken);

        var rows = new List<TrialBalanceRowDto>();
        decimal grandTotalDebit = 0;
        decimal grandTotalCredit = 0;

        foreach (var acc in accounts)
        {
            var accLines = lines.Where(l => l.AccountId == acc.Id).ToList();
            var sumDebit = accLines.Sum(l => l.DebitAmount);
            var sumCredit = accLines.Sum(l => l.CreditAmount);

            // Asset & Expense normally have debit balances; Liability, Equity, Revenue credit
            decimal net = (acc.Classification == AccountClassification.Asset || acc.Classification == AccountClassification.Expense)
                ? (sumDebit - sumCredit)
                : (sumCredit - sumDebit);

            rows.Add(new TrialBalanceRowDto(
                acc.AccountCode,
                acc.Name,
                acc.Classification,
                sumDebit,
                sumCredit,
                net
            ));

            grandTotalDebit += sumDebit;
            grandTotalCredit += sumCredit;
        }

        return new TrialBalanceReportDto(
            rows,
            grandTotalDebit,
            grandTotalCredit,
            Math.Abs(grandTotalDebit - grandTotalCredit) < 0.01m
        );
    }
}
