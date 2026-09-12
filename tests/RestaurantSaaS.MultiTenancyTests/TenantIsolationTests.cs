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
        var branchA2 = new Branch { Id = Guid.NewGuid(), RestaurantId = restaurantA.Id, BranchName = "Alpha Uptown", BranchCode = "A2" };
        var branchB1 = new Branch { Id = Guid.NewGuid(), RestaurantId = restaurantB.Id, BranchName = "Beta Central", BranchCode = "B1" };

        var userA = new User { Id = Guid.NewGuid(), RestaurantId = restaurantA.Id, Email = "user@alpha.com", NormalizedEmail = "USER@ALPHA.COM", FullName = "User Alpha" };
        var userB = new User { Id = Guid.NewGuid(), RestaurantId = restaurantB.Id, Email = "user@beta.com", NormalizedEmail = "USER@BETA.COM", FullName = "User Beta" };

        // Catalog & Tables for A & B
        var catA = new Category { Id = Guid.NewGuid(), RestaurantId = restaurantA.Id, Name = "Burgers" };
        var catB = new Category { Id = Guid.NewGuid(), RestaurantId = restaurantB.Id, Name = "Pizzas" };

        var prodA = new Product { Id = Guid.NewGuid(), RestaurantId = restaurantA.Id, CategoryId = catA.Id, Name = "Alpha Zinger", SKU = "ALPHA-ZNG", BasePrice = 10.0m };
        var prodB = new Product { Id = Guid.NewGuid(), RestaurantId = restaurantB.Id, CategoryId = catB.Id, Name = "Beta Pepperoni", SKU = "BETA-PEP", BasePrice = 15.0m };

        var tableA = new RestaurantTable { Id = Guid.NewGuid(), RestaurantId = restaurantA.Id, BranchId = branchA1.Id, TableNumber = "T1", Capacity = 4 };
        var tableB = new RestaurantTable { Id = Guid.NewGuid(), RestaurantId = restaurantB.Id, BranchId = branchB1.Id, TableNumber = "T1", Capacity = 4 };

        // Save into DB without tenant filter
        _tenantService.SetTenant(Guid.Empty);
        _context.Restaurants.AddRange(restaurantA, restaurantB);
        _context.Branches.AddRange(branchA1, branchA2, branchB1);
        _context.Users.AddRange(userA, userB);
        _context.Categories.AddRange(catA, catB);
        _context.Products.AddRange(prodA, prodB);
        _context.RestaurantTables.AddRange(tableA, tableB);
        await _context.SaveChangesAsync();

        // ACT 1: Switch context to Restaurant A
        _tenantService.SetTenant(restaurantA.Id);

        var branchesForA = await _context.Branches.ToListAsync();
        var usersForA = await _context.Users.ToListAsync();
        var productsForA = await _context.Products.ToListAsync();
        var tablesForA = await _context.RestaurantTables.ToListAsync();

        // ASSERT 1: Must only contain Restaurant A records
        branchesForA.Should().HaveCount(2);
        branchesForA.Select(b => b.BranchCode).Should().NotContain("B1");

        usersForA.Should().HaveCount(1);
        usersForA.Single().Email.Should().Be("user@alpha.com");

        productsForA.Should().HaveCount(1);
        productsForA.Single().Name.Should().Be("Alpha Zinger");

        tablesForA.Should().HaveCount(1);
        tablesForA.Single().Id.Should().Be(tableA.Id);

        // ACT 2: Attempting to query Restaurant B's product by specific ID while scoped to Restaurant A
        var productBQueriedByA = await _context.Products.FirstOrDefaultAsync(p => p.Id == prodB.Id);

        // ASSERT 2: The global query filter must hide Restaurant B's entity completely
        productBQueriedByA.Should().BeNull();

        // ACT 3: Switch context to Restaurant B
        _tenantService.SetTenant(restaurantB.Id);

        var productsForB = await _context.Products.ToListAsync();
        var tablesForB = await _context.RestaurantTables.ToListAsync();

        // ASSERT 3: Must only contain Restaurant B records
        productsForB.Should().HaveCount(1);
        productsForB.Single().Name.Should().Be("Beta Pepperoni");

        tablesForB.Should().HaveCount(1);
        tablesForB.Single().Id.Should().Be(tableB.Id);
    }
}
