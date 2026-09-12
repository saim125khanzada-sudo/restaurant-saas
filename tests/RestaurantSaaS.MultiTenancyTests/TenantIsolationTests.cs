using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RestaurantSaaS.Domain.Entities;
using RestaurantSaaS.Domain.Enums;
using RestaurantSaaS.Infrastructure.Data;
using RestaurantSaaS.Infrastructure.Services;
using Xunit;

namespace RestaurantSaaS.MultiTenancyTests;

public class TenantIsolationTests
{
    private readonly CurrentTenantService _tenantService;
    private readonly ApplicationDbContext _context;

    public TenantIsolationTests()
    {
        _tenantService = new CurrentTenantService();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options, _tenantService);
    }

    [Fact]
    public async Task Query_WhenAuthenticatedAsRestaurantA_MustNeverReturnRestaurantBData()
    {
        // ARRANGE: Setup Restaurant A and Restaurant B
        var restaurantA = new Restaurant
        {
            Id = Guid.NewGuid(),
            Name = "Restaurant Alpha",
            Code = "ALPHA",
            ContactEmail = "admin@alpha.com"
        };

        var restaurantB = new Restaurant
        {
            Id = Guid.NewGuid(),
            Name = "Restaurant Beta",
            Code = "BETA",
            ContactEmail = "admin@beta.com"
        };

        var branchA1 = new Branch { Id = Guid.NewGuid(), RestaurantId = restaurantA.Id, BranchName = "Alpha Downtown", BranchCode = "A1" };
        var branchB1 = new Branch { Id = Guid.NewGuid(), RestaurantId = restaurantB.Id, BranchName = "Beta Central", BranchCode = "B1" };

        var userA = new User { Id = Guid.NewGuid(), RestaurantId = restaurantA.Id, Email = "user@alpha.com", NormalizedEmail = "USER@ALPHA.COM", FullName = "User Alpha" };
        var userB = new User { Id = Guid.NewGuid(), RestaurantId = restaurantB.Id, Email = "user@beta.com", NormalizedEmail = "USER@BETA.COM", FullName = "User Beta" };

        var prodA = new Product { Id = Guid.NewGuid(), RestaurantId = restaurantA.Id, Name = "Alpha Zinger", SKU = "ALPHA-ZNG", BasePrice = 10.0m };
        var prodB = new Product { Id = Guid.NewGuid(), RestaurantId = restaurantB.Id, Name = "Beta Pepperoni", SKU = "BETA-PEP", BasePrice = 15.0m };

        var orderA = new Order
        {
            Id = Guid.NewGuid(),
            RestaurantId = restaurantA.Id,
            BranchId = branchA1.Id,
            OrderNumber = "ORD-ALPHA-001",
            Status = OrderStatus.Created,
            PaymentStatus = PaymentStatus.Paid,
            GrandTotal = 25.00m,
            IdempotencyKey = Guid.NewGuid()
        };

        var orderB = new Order
        {
            Id = Guid.NewGuid(),
            RestaurantId = restaurantB.Id,
            BranchId = branchB1.Id,
            OrderNumber = "ORD-BETA-001",
            Status = OrderStatus.Created,
            PaymentStatus = PaymentStatus.Unpaid,
            GrandTotal = 30.00m,
            IdempotencyKey = Guid.NewGuid()
        };

        var paymentA = new Payment
        {
            Id = Guid.NewGuid(),
            RestaurantId = restaurantA.Id,
            OrderId = orderA.Id,
            Amount = 25.00m,
            Method = PaymentMethod.Card
        };

        // Save into DB without tenant filter
        _tenantService.SetTenant(Guid.Empty);
        _context.Restaurants.AddRange(restaurantA, restaurantB);
        _context.Branches.AddRange(branchA1, branchB1);
        _context.Users.AddRange(userA, userB);
        _context.Products.AddRange(prodA, prodB);
        _context.Orders.AddRange(orderA, orderB);
        _context.Payments.Add(paymentA);
        await _context.SaveChangesAsync();

        // ACT 1: Switch context to Restaurant A
        _tenantService.SetTenant(restaurantA.Id);

        var ordersForA = await _context.Orders.ToListAsync();
        var paymentsForA = await _context.Payments.ToListAsync();

        // ASSERT 1: Must only contain Restaurant A records
        ordersForA.Should().HaveCount(1);
        ordersForA.Single().OrderNumber.Should().Be("ORD-ALPHA-001");
        paymentsForA.Should().HaveCount(1);
        paymentsForA.Single().Amount.Should().Be(25.00m);

        // ACT 2: Query Order B by specific ID while scoped to Restaurant A
        var orderBQueriedByA = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderB.Id);
        orderBQueriedByA.Should().BeNull("Restaurant A must never be able to access Restaurant B's order.");

        // ACT 3: Switch context to Restaurant B
        _tenantService.SetTenant(restaurantB.Id);

        var ordersForB = await _context.Orders.ToListAsync();
        var paymentsForB = await _context.Payments.ToListAsync();

        // ASSERT 3: Must only contain Restaurant B records
        ordersForB.Should().HaveCount(1);
        ordersForB.Single().OrderNumber.Should().Be("ORD-BETA-001");
        paymentsForB.Should().BeEmpty("Restaurant B has no payments.");
    }
}
