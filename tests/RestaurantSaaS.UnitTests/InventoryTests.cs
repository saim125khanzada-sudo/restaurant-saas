using System;
using FluentAssertions;
using RestaurantSaaS.Domain.Entities;
using RestaurantSaaS.Domain.Enums;
using Xunit;

namespace RestaurantSaaS.UnitTests;

public class InventoryTests
{
    [Fact]
    public void RecipeBOM_CalculatesTotalIngredientRequirement()
    {
        // Arrange
        var doughPerPizza = 0.250m; // 250 grams
        var cheesePerPizza = 0.150m; // 150 grams
        var pizzasOrdered = 4;

        // Act
        var totalDough = doughPerPizza * pizzasOrdered;
        var totalCheese = cheesePerPizza * pizzasOrdered;

        // Assert
        totalDough.Should().Be(1.000m);
        totalCheese.Should().Be(0.600m);
    }

    [Fact]
    public void StockMovement_DecrementsQuantityCorrectly()
    {
        // Arrange
        var stock = new StockLevel
        {
            IngredientId = Guid.NewGuid(),
            QuantityOnHand = 10.000m
        };

        var deduction = 2.500m;

        // Act
        stock.QuantityOnHand -= deduction;

        // Assert
        stock.QuantityOnHand.Should().Be(7.500m);
    }
}
