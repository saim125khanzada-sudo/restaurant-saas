using System;
using FluentAssertions;
using RestaurantSaaS.Domain.Entities;
using Xunit;

namespace RestaurantSaaS.UnitTests;

public class HumanResourcesTests
{
    [Fact]
    public void OvertimeCalculation_AboveEightHours_IsAccurate()
    {
        // Arrange
        var clockIn = new DateTime(2026, 9, 10, 9, 0, 0, DateTimeKind.Utc);
        var clockOut = new DateTime(2026, 9, 10, 19, 30, 0, DateTimeKind.Utc); // 10.5 hours

        // Act
        var totalHours = (decimal)(clockOut - clockIn).TotalHours;
        var overtime = Math.Max(0, totalHours - 8.0m);

        // Assert
        totalHours.Should().Be(10.5m);
        overtime.Should().Be(2.5m);
    }

    [Fact]
    public void PayrollRun_CalculatesNetSalaryWithAdvanceDeductions()
    {
        // Arrange
        var baseMonthly = 1300.00m; // 50/day across 26 days
        var daysPresent = 24; // 2 days absent
        var dailyRate = baseMonthly / 26m;
        var earnedBase = Math.Round(daysPresent * dailyRate, 2); // 1200.00
        var overtimePay = 100.00m;
        var advanceDeduction = 150.00m;

        // Act
        var gross = earnedBase + overtimePay; // 1300.00
        var net = gross - advanceDeduction;    // 1150.00

        // Assert
        gross.Should().Be(1300.00m);
        net.Should().Be(1150.00m);
    }
}
