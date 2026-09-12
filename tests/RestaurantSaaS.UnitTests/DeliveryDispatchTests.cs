using System;
using FluentAssertions;
using RestaurantSaaS.Domain.Entities;
using RestaurantSaaS.Domain.Enums;
using Xunit;

namespace RestaurantSaaS.UnitTests;

public class DeliveryDispatchTests
{
    [Fact]
    public void DeliveryLifecycle_TransitionSequence_IsValid()
    {
        // Arrange
        var dispatch = new DeliveryDispatch
        {
            OrderId = Guid.NewGuid(),
            RiderId = Guid.NewGuid(),
            Status = DeliveryStatus.Assigned,
            CashToCollect = 45.50m
        };

        // Act & Assert 1: Accept
        dispatch.Status = DeliveryStatus.Accepted;
        dispatch.AcceptedAt = DateTime.UtcNow;
        dispatch.Status.Should().Be(DeliveryStatus.Accepted);

        // Act & Assert 2: Picked Up
        dispatch.Status = DeliveryStatus.PickedUp;
        dispatch.PickedUpAt = DateTime.UtcNow;
        dispatch.Status.Should().Be(DeliveryStatus.PickedUp);

        // Act & Assert 3: Delivered with COD cash collection
        dispatch.Status = DeliveryStatus.Delivered;
        dispatch.DeliveredAt = DateTime.UtcNow;
        dispatch.CashCollected = 45.50m;
        dispatch.IsCashCollected = true;

        dispatch.Status.Should().Be(DeliveryStatus.Delivered);
        dispatch.CashCollected.Should().Be(dispatch.CashToCollect);
        dispatch.IsCashCollected.Should().BeTrue();
    }

    [Fact]
    public void RiderCashReconciliation_ZeroDiscrepancy_IsApproved()
    {
        // Arrange
        var totalExpected = 150.00m;
        var totalSubmitted = 150.00m;
        var discrepancy = totalSubmitted - totalExpected;

        var recon = new RiderCashReconciliation
        {
            RiderId = Guid.NewGuid(),
            TotalDeliveries = 4,
            TotalCashExpected = totalExpected,
            TotalCashSubmitted = totalSubmitted,
            DiscrepancyAmount = discrepancy,
            Status = discrepancy == 0 ? ReconciliationStatus.Approved : ReconciliationStatus.DiscrepancyReported
        };

        // Assert
        recon.DiscrepancyAmount.Should().Be(0);
        recon.Status.Should().Be(ReconciliationStatus.Approved);
    }
}
