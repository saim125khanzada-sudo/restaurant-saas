using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using RestaurantSaaS.Application.Taxation.Services;
using RestaurantSaaS.Domain.Entities;
using RestaurantSaaS.Domain.Enums;
using Xunit;

namespace RestaurantSaaS.UnitTests;

public class TaxationTests
{
    [Theory]
    [InlineData(100.00, 5.0, 5.00, 105.00)]
    [InlineData(2500.00, 16.0, 400.00, 2900.00)]
    [InlineData(1500.00, 15.0, 225.00, 1725.00)]
    [InlineData(0.00, 15.0, 0.00, 0.00)]
    public void TaxCalculation_CalculatesTaxAndGrandTotalAccurately(
        decimal subtotal, decimal rate, decimal expectedTax, decimal expectedTotal)
    {
        // Act
        var taxAmount = Math.Round(subtotal * (rate / 100m), 2, MidpointRounding.AwayFromZero);
        var grandTotal = subtotal + taxAmount;

        // Assert
        taxAmount.Should().Be(expectedTax);
        grandTotal.Should().Be(expectedTotal);
    }

    [Fact]
    public async Task FbrFiscalService_GeneratesCompliantInvoiceNumberAndQrPayload()
    {
        // Arrange
        var service = new FbrFiscalService();
        var order = new Order
        {
            Id = Guid.NewGuid(),
            RestaurantId = Guid.NewGuid(),
            BranchId = Guid.NewGuid(),
            OrderNumber = "ORD-20260913-0001",
            Subtotal = 1000.00m,
            TaxTotal = 50.00m,
            GrandTotal = 1050.00m
        };

        // Act
        var result = await service.FiscalizeInvoiceAsync(order, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.FbrInvoiceNumber.Should().StartWith("POS-");
        result.QrCodeData.Should().Contain("FBR:INVOICE=");
        result.QrCodeData.Should().Contain("TOTAL=1050.00");
        result.QrCodeData.Should().Contain("TAX=50.00");
        result.ResponsePayload.Should().Contain("VERIFIED_ONLINE");
    }

    [Fact]
    public void FiscalInvoiceRecord_SetsDefaultStatusToPending()
    {
        // Arrange & Act
        var record = new FiscalInvoiceRecord
        {
            RestaurantId = Guid.NewGuid(),
            BranchId = Guid.NewGuid(),
            OrderId = Guid.NewGuid()
        };

        // Assert
        record.Status.Should().Be(FiscalSyncStatus.Pending);
        record.RetryCount.Should().Be(0);
    }
}
