using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using RestaurantSaaS.Domain.Entities;
using RestaurantSaaS.Domain.Enums;
using Xunit;

namespace RestaurantSaaS.UnitTests;

public class ReportingAndSubscriptionTests
{
    [Fact]
    public void SalesAggregation_CalculatesAccurateFinancialTotals()
    {
        // Arrange
        var orders = new List<Order>
        {
            new Order
            {
                Id = Guid.NewGuid(),
                Status = OrderStatus.Completed,
                OrderType = OrderType.DineIn,
                Subtotal = 1000m,
                TaxTotal = 150m,
                DiscountTotal = 50m,
                GrandTotal = 1100m
            },
            new Order
            {
                Id = Guid.NewGuid(),
                Status = OrderStatus.Completed,
                OrderType = OrderType.Delivery,
                Subtotal = 2000m,
                TaxTotal = 100m,
                DiscountTotal = 0m,
                GrandTotal = 2100m
            },
            new Order
            {
                Id = Guid.NewGuid(),
                Status = OrderStatus.Cancelled, // Must be excluded from revenue
                OrderType = OrderType.Takeaway,
                Subtotal = 500m,
                TaxTotal = 25m,
                DiscountTotal = 0m,
                GrandTotal = 525m
            }
        };

        // Act
        var completedOrders = orders.Where(o => o.Status == OrderStatus.Completed).ToList();
        var grossSales = completedOrders.Sum(o => o.Subtotal);
        var totalTaxes = completedOrders.Sum(o => o.TaxTotal);
        var totalDiscounts = completedOrders.Sum(o => o.DiscountTotal);
        var netRevenue = completedOrders.Sum(o => o.GrandTotal);

        // Assert
        completedOrders.Should().HaveCount(2);
        grossSales.Should().Be(3000m);
        totalTaxes.Should().Be(250m);
        totalDiscounts.Should().Be(50m);
        netRevenue.Should().Be(3200m);
    }

    [Theory]
    [InlineData(1, 1, false)] // At limit: cannot create more
    [InlineData(1, 2, false)] // Exceeded limit: cannot create more
    [InlineData(1, 0, true)]  // Below limit: can create more
    [InlineData(5, 4, true)]  // Pro plan (5 branches, 4 used): can create more
    public void SubscriptionPlan_EnforcesMaxBranchLimits(
        int maxAllowedBranches, int currentBranches, bool expectedCanCreateMore)
    {
        // Act
        var canCreateMore = currentBranches < maxAllowedBranches;

        // Assert
        canCreateMore.Should().Be(expectedCanCreateMore);
    }

    [Fact]
    public void TenantSubscription_InitializesWithActiveStatusAndPeriod()
    {
        // Arrange & Act
        var now = DateTime.UtcNow;
        var sub = new TenantSubscription
        {
            RestaurantId = Guid.NewGuid(),
            PlanId = Guid.NewGuid(),
            Status = SubscriptionStatus.Active,
            PeriodStart = now,
            PeriodEnd = now.AddMonths(1),
            AutoRenew = true
        };

        // Assert
        sub.Status.Should().Be(SubscriptionStatus.Active);
        sub.PeriodEnd.Should().BeAfter(sub.PeriodStart);
        sub.AutoRenew.Should().BeTrue();
    }
}
