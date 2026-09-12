using System;
using System.Collections.Generic;
using FluentAssertions;
using RestaurantSaaS.Application.Security;
using RestaurantSaaS.Domain.Entities;
using Xunit;

namespace RestaurantSaaS.UnitTests;

public class SecurityHardeningTests
{
    [Fact]
    public void AuditLog_ComputesConsistentSha256Hash()
    {
        // Arrange
        var restaurantId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var prevHash = "0000000000000000000000000000000000000000000000000000000000000000";

        // Act
        var hash1 = AuditLogService.ComputeHash(restaurantId, "ORDER_CREATED", "Order", "ord-101", null, "{ 'total': 500 }", now, prevHash);
        var hash2 = AuditLogService.ComputeHash(restaurantId, "ORDER_CREATED", "Order", "ord-101", null, "{ 'total': 500 }", now, prevHash);

        // Assert
        hash1.Should().NotBeNullOrWhiteSpace();
        hash1.Length.Should().Be(64); // Standard SHA-256 hex string length
        hash1.Should().Be(hash2);
    }

    [Fact]
    public void AuditLog_DetectsDirectDataTampering()
    {
        // Arrange
        var restaurantId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var prevHash = "0000000000000000000000000000000000000000000000000000000000000000";

        var legitimateHash = AuditLogService.ComputeHash(restaurantId, "SALES_RECORD", "Order", "ord-101", null, "{ 'total': 1000 }", now, prevHash);

        // Act: Attacker modifies the logged total from 1000 to 100 in the DB directly
        var tamperedHash = AuditLogService.ComputeHash(restaurantId, "SALES_RECORD", "Order", "ord-101", null, "{ 'total': 100 }", now, prevHash);

        // Assert
        tamperedHash.Should().NotBe(legitimateHash);
    }

    [Fact]
    public void AuditLog_ChainBreaks_IfPreviousHashIsAltered()
    {
        // Arrange
        var restaurantId = Guid.NewGuid();
        var t1 = DateTimeOffset.UtcNow;
        var t2 = t1.AddSeconds(10);
        var genesis = "0000000000000000000000000000000000000000000000000000000000000000";

        var hashBlock1 = AuditLogService.ComputeHash(restaurantId, "USER_LOGIN", "User", "u-1", null, null, t1, genesis);
        var hashBlock2 = AuditLogService.ComputeHash(restaurantId, "ORDER_PAID", "Order", "o-1", null, null, t2, hashBlock1);

        // Act: Alter block 1 hash
        var fakeBlock1Hash = "ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff";
        var recalculatedBlock2 = AuditLogService.ComputeHash(restaurantId, "ORDER_PAID", "Order", "o-1", null, null, t2, fakeBlock1Hash);

        // Assert
        recalculatedBlock2.Should().NotBe(hashBlock2);
    }
}

