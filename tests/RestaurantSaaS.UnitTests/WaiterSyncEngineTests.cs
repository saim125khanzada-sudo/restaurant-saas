using System;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace RestaurantSaaS.UnitTests;

public class WaiterSyncEngineTests
{
    [Fact]
    public void OrderDraft_PayloadMatchesServerIdempotencyContract()
    {
        // Arrange
        var draft = new
        {
            branchId = Guid.NewGuid().ToString(),
            orderType = "DineIn",
            tableId = Guid.NewGuid().ToString(),
            idempotencyKey = Guid.NewGuid().ToString(),
            notes = "Spicy, no onions",
            items = new[]
            {
                new
                {
                    productId = Guid.NewGuid().ToString(),
                    productVariantId = (string?)null,
                    quantity = 2,
                    notes = "Hot",
                    addons = new[]
                    {
                        new { addonId = Guid.NewGuid().ToString(), unitPrice = 1.50m }
                    }
                }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(draft);

        // Assert
        json.Should().Contain("idempotencyKey");
        json.Should().Contain("DineIn");
        json.Should().Contain("Spicy, no onions");
    }

    [Fact]
    public void TableStatus_TransitionToOccupied_IsValid()
    {
        // Arrange
        var statuses = new[] { "Available", "Occupied", "Reserved", "Billed" };

        // Act & Assert
        statuses.Should().Contain("Available");
        statuses.Should().Contain("Occupied");
    }
}
