using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RestaurantSaaS.Application.Interfaces;
using RestaurantSaaS.Domain.Entities;
using RestaurantSaaS.Domain.Enums;
using RestaurantSaaS.Domain.Exceptions;

namespace RestaurantSaaS.Application.Accounting.Services;

public interface IAccountingPostingService
{
    Task<Guid> PostOrderSalesJournalEntryAsync(Guid orderId, CancellationToken cancellationToken = default);
}

public class AccountingPostingService : IAccountingPostingService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenantService _tenantService;

    public AccountingPostingService(IApplicationDbContext context, ICurrentTenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    public async Task<Guid> PostOrderSalesJournalEntryAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken)
            ?? throw new NotFoundException($"Order {orderId} not found.");

        var restaurantId = _tenantService.RestaurantId ?? order.RestaurantId;

        // Fetch standard accounts or fallback to default codes
        var cashAccount = await GetOrCreateAccountAsync(restaurantId, "1010", "Cash on Hand", AccountClassification.Asset);
        var salesAccount = await GetOrCreateAccountAsync(restaurantId, "4010", "Food & Beverage Sales", AccountClassification.Revenue);
        var taxAccount = await GetOrCreateAccountAsync(restaurantId, "2020", "Sales Tax Payable", AccountClassification.Liability);

        var journal = new JournalEntry
        {
            RestaurantId = restaurantId,
            BranchId = order.BranchId,
            EntryNumber = $"JE-ORD-{order.OrderNumber}",
            PostingDate = DateTime.UtcNow,
            Description = $"Sales revenue posting for Order #{order.OrderNumber}",
            Status = JournalEntryStatus.Posted,
            SourceDocumentId = order.Id,
            SourceDocumentType = "Order",
            TotalDebit = order.GrandTotal,
            TotalCredit = order.GrandTotal
        };

        // 1. Debit: Cash on Hand (or Receivables)
        journal.Lines.Add(new JournalLine
        {
            RestaurantId = restaurantId,
            AccountId = cashAccount.Id,
            DebitAmount = order.GrandTotal,
            CreditAmount = 0,
            LineDescription = $"Cash collected for Order #{order.OrderNumber}"
        });

        // 2. Credit: Sales Revenue (Subtotal)
        journal.Lines.Add(new JournalLine
        {
            RestaurantId = restaurantId,
            AccountId = salesAccount.Id,
            DebitAmount = 0,
            CreditAmount = order.Subtotal,
            LineDescription = $"Food sales revenue"
        });

        // 3. Credit: Sales Tax Payable (TaxTotal)
        if (order.TaxTotal > 0)
        {
            journal.Lines.Add(new JournalLine
            {
                RestaurantId = restaurantId,
                AccountId = taxAccount.Id,
                DebitAmount = 0,
                CreditAmount = order.TaxTotal,
                LineDescription = $"Tax collected"
            });
        }

        // Strict validation: Debits must equal Credits
        var sumDebit = journal.Lines.Sum(l => l.DebitAmount);
        var sumCredit = journal.Lines.Sum(l => l.CreditAmount);
        if (sumDebit != sumCredit)
        {
            throw new DomainException($"Unbalanced journal entry! Total Debits: {sumDebit}, Total Credits: {sumCredit}");
        }

        _context.JournalEntries.Add(journal);
        await _context.SaveChangesAsync(cancellationToken);

        return journal.Id;
    }

    private async Task<Account> GetOrCreateAccountAsync(Guid restaurantId, string code, string name, AccountClassification classification)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.RestaurantId == restaurantId && a.AccountCode == code);

        if (account == null)
        {
            account = new Account
            {
                RestaurantId = restaurantId,
                AccountCode = code,
                Name = name,
                Classification = classification,
                Currency = "USD"
            };
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();
        }

        return account;
    }
}
