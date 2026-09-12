namespace RestaurantSaaS.Domain.Enums;

public enum AccountClassification
{
    Asset = 1,
    Liability = 2,
    Equity = 3,
    Revenue = 4,
    Expense = 5
}

public enum JournalEntryStatus
{
    Draft = 1,
    Posted = 2,
    Reversed = 3
}

public enum CashRegisterSessionStatus
{
    Open = 1,
    Closed = 2,
    Audited = 3
}
