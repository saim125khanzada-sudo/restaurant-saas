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

        // Save into DB without tenant filter
        _tenantService.SetTenant(Guid.Empty); // Bypass during seed
        _context.Restaurants.AddRange(restaurantA, restaurantB);
        _context.Branches.AddRange(branchA1, branchA2, branchB1);
        _context.Users.AddRange(userA, userB);
        await _context.SaveChangesAsync();

        // ACT 1: Switch context to Restaurant A
        _tenantService.SetTenant(restaurantA.Id);

        var branchesForA = await _context.Branches.ToListAsync();
        var usersForA = await _context.Users.ToListAsync();

        // ASSERT 1: Must only contain Restaurant A records
        branchesForA.Should().HaveCount(2);
        branchesForA.Select(b => b.BranchCode).Should().Contain(new[] { "A1", "A2" });
        branchesForA.Select(b => b.BranchCode).Should().NotContain("B1");

        usersForA.Should().HaveCount(1);
        usersForA.Single().Email.Should().Be("user@alpha.com");

        // ACT 2: Attempting to query Restaurant B's branch by specific ID while scoped to Restaurant A
        var branchBQueriedByA = await _context.Branches.FirstOrDefaultAsync(b => b.Id == branchB1.Id);

        // ASSERT 2: The global query filter must hide Restaurant B's entity completely
        branchBQueriedByA.Should().BeNull();

        // ACT 3: Switch context to Restaurant B
        _tenantService.SetTenant(restaurantB.Id);

        var branchesForB = await _context.Branches.ToListAsync();
        var usersForB = await _context.Users.ToListAsync();

        // ASSERT 3: Must only contain Restaurant B records
        branchesForB.Should().HaveCount(1);
        branchesForB.Single().BranchCode.Should().Be("B1");
        usersForB.Should().HaveCount(1);
        usersForB.Single().Email.Should().Be("user@beta.com");
    }
}
