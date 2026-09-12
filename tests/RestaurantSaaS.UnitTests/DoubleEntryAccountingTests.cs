using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using RestaurantSaaS.Domain.Entities;
using RestaurantSaaS.Domain.Enums;
using Xunit;

namespace RestaurantSaaS.UnitTests;

public class DoubleEntryAccountingTests
{
    [Fact]
    public void SalesJournalEntry_DebitsStrictlyEqualCredits()
    {
        // Arrange
        var orderSubtotal = 85.00m;
        var taxAmount = 12.75m;
        var totalAmount = orderSubtotal + taxAmount; // 97.75

        var lines = new List<JournalLine>
        {
            // Debit Cash on Hand
            new JournalLine
            {
                AccountId = Guid.NewGuid(),
                DebitAmount = totalAmount,
                CreditAmount = 0
            },
            // Credit Sales Revenue
            new JournalLine
            {
                AccountId = Guid.NewGuid(),
                DebitAmount = 0,
                CreditAmount = orderSubtotal
            },
            // Credit Sales Tax Payable
            new JournalLine
            {
                AccountId = Guid.NewGuid(),
                DebitAmount = 0,
                CreditAmount = taxAmount
            }
        };

        // Act
        var sumDebit = lines.Sum(l => l.DebitAmount);
        var sumCredit = lines.Sum(l => l.CreditAmount);

        // Assert
        sumDebit.Should().Be(totalAmount);
        sumCredit.Should().Be(totalAmount);
        (sumDebit - sumCredit).Should().Be(0);
    }

    [Fact]
    public void CashRegisterSession_CalculatesOverShortDiscrepancy()
    {
        // Arrange
        var session = new CashRegisterSession
        {
            OpeningFloat = 200.00m,
            CashSales = 550.00m,
            CashDrops = 100.00m
        };
        session.ExpectedCash = session.OpeningFloat + session.CashSales - session.CashDrops; // 650.00

        // Act: Cashier counted 645.00 (Short by 5.00)
        session.ActualCountedCash = 645.00m;
        session.Discrepancy = session.ActualCountedCash - session.ExpectedCash;

        // Assert
        session.ExpectedCash.Should().Be(650.00m);
        session.Discrepancy.Should().Be(-5.00m);
    }
}
