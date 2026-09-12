using System;
using System.Collections.Generic;
using RestaurantSaaS.Domain.Enums;
using RestaurantSaaS.SharedKernel.Common;
using RestaurantSaaS.SharedKernel.Interfaces;

namespace RestaurantSaaS.Domain.Entities;

public class Account : BaseEntity, IMultiTenantEntity
{
    public Guid RestaurantId { get; set; }
    public string AccountCode { get; set; } = string.Empty; // e.g., "1010", "4010"
    public string Name { get; set; } = string.Empty;        // e.g., "Cash on Hand", "Food Sales"
    public AccountClassification Classification { get; set; }
    public string Currency { get; set; } = "USD";
    public bool IsActive { get; set; } = true;
    public string? Description { get; set; }
}

public class JournalEntry : BaseEntity, IMultiTenantEntity
{
    public Guid RestaurantId { get; set; }
    public Guid? BranchId { get; set; }
    public string EntryNumber { get; set; } = string.Empty;
    public DateTime PostingDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public JournalEntryStatus Status { get; set; } = JournalEntryStatus.Draft;
    public Guid? SourceDocumentId { get; set; } // OrderId, PurchaseOrderId, etc.
    public string? SourceDocumentType { get; set; }

    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }

    // Navigation
    public List<JournalLine> Lines { get; set; } = new();
}

public class JournalLine : BaseEntity, IMultiTenantEntity
{
    public Guid RestaurantId { get; set; }
    public Guid JournalEntryId { get; set; }
    public Guid AccountId { get; set; }
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public string? LineDescription { get; set; }

    // Navigation
    public Account? Account { get; set; }
}

public class CashRegisterSession : BaseEntity, IMultiTenantEntity
{
    public Guid RestaurantId { get; set; }
    public Guid BranchId { get; set; }
    public Guid CashierUserId { get; set; }
    public DateTime OpenedAt { get; set; }
    public DateTime? ClosedAt { get; set; }

    public decimal OpeningFloat { get; set; }
    public decimal CashSales { get; set; }
    public decimal CashDrops { get; set; } // Cash removed to safe
    public decimal ExpectedCash { get; set; } // OpeningFloat + CashSales - CashDrops
    public decimal? ActualCountedCash { get; set; }
    public decimal? Discrepancy { get; set; } // ActualCountedCash - ExpectedCash

    public CashRegisterSessionStatus Status { get; set; } = CashRegisterSessionStatus.Open;
    public string? Notes { get; set; }

    // Navigation
    public User? CashierUser { get; set; }
}
