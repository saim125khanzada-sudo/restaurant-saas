using FluentAssertions;
using RestaurantSaaS.Domain.Enums;
using RestaurantSaaS.Domain.Services;
using Xunit;

namespace RestaurantSaaS.UnitTests;

public class OrderStateMachineTests
{
    [Theory]
    [InlineData(OrderStatus.Created, OrderStatus.Submitted, true)]
    [InlineData(OrderStatus.Submitted, OrderStatus.Confirmed, true)]
    [InlineData(OrderStatus.Confirmed, OrderStatus.KitchenPreparing, true)]
    [InlineData(OrderStatus.KitchenPreparing, OrderStatus.Ready, true)]
    [InlineData(OrderStatus.Ready, OrderStatus.Served, true)]
    [InlineData(OrderStatus.Served, OrderStatus.Completed, true)]
    [InlineData(OrderStatus.Completed, OrderStatus.Paid, true)]
    [InlineData(OrderStatus.Paid, OrderStatus.Refunded, true)]
    public void DineInFlow_ValidTransitions_ShouldSucceed(OrderStatus current, OrderStatus next, bool expected)
    {
        var result = OrderStateMachine.CanTransition(current, next);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(OrderStatus.Created, OrderStatus.Delivered)]
    [InlineData(OrderStatus.Created, OrderStatus.Paid)]
    [InlineData(OrderStatus.Paid, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Delivered, OrderStatus.KitchenPreparing)]
    [InlineData(OrderStatus.Completed, OrderStatus.Created)]
    public void InvalidTransitions_ShouldBeBlocked(OrderStatus current, OrderStatus next)
    {
        var result = OrderStateMachine.CanTransition(current, next);
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(OrderStatus.Confirmed, OrderStatus.ReadyForPickup, true)]
    [InlineData(OrderStatus.ReadyForPickup, OrderStatus.RiderAssigned, true)]
    [InlineData(OrderStatus.RiderAssigned, OrderStatus.PickedUp, true)]
    [InlineData(OrderStatus.PickedUp, OrderStatus.OutForDelivery, true)]
    [InlineData(OrderStatus.OutForDelivery, OrderStatus.Delivered, true)]
    [InlineData(OrderStatus.Delivered, OrderStatus.CodSettled, true)]
    [InlineData(OrderStatus.CodSettled, OrderStatus.Paid, true)]
    public void DeliveryFlow_ValidTransitions_ShouldSucceed(OrderStatus current, OrderStatus next, bool expected)
    {
        var result = OrderStateMachine.CanTransition(current, next);
        result.Should().Be(expected);
    }
}
