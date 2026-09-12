using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantSaaS.Application.Interfaces;
using RestaurantSaaS.Domain.Entities;
using RestaurantSaaS.Domain.Enums;

namespace RestaurantSaaS.Application.Features.Auth.Commands;

public record SeedDemoTenantResult(
    Guid RestaurantId,
    Guid BranchId,
    string RestaurantName,
    string AdminEmail,
    string AdminPassword,
    int CategoriesCount,
    int ProductsCount,
    int TablesCount
);

public record SeedDemoTenantCommand() : IRequest<SeedDemoTenantResult>;

public class SeedDemoTenantCommandHandler : IRequestHandler<SeedDemoTenantCommand, SeedDemoTenantResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _hasher;

    public SeedDemoTenantCommandHandler(IApplicationDbContext context, IPasswordHasher hasher)
    {
        _context = context;
        _hasher = hasher;
    }

    public async Task<SeedDemoTenantResult> Handle(SeedDemoTenantCommand request, CancellationToken cancellationToken)
    {
        // Check if demo already exists
        var existingRestaurant = await _context.Restaurants
            .FirstOrDefaultAsync(r => r.Code == "DEMO-REST-01", cancellationToken);

        if (existingRestaurant != null)
        {
            var existingBranch = await _context.Branches.FirstAsync(b => b.RestaurantId == existingRestaurant.Id, cancellationToken);
            var catCount = await _context.Categories.CountAsync(c => c.RestaurantId == existingRestaurant.Id, cancellationToken);
            var prodCount = await _context.Products.CountAsync(p => p.RestaurantId == existingRestaurant.Id, cancellationToken);
            var tblCount = await _context.RestaurantTables.CountAsync(t => t.RestaurantId == existingRestaurant.Id, cancellationToken);

            return new SeedDemoTenantResult(
                existingRestaurant.Id,
                existingBranch.Id,
                existingRestaurant.Name,
                "admin@demo-bistro.com",
                "DemoPass123!",
                catCount,
                prodCount,
                tblCount
            );
        }

        // 1. Create Demo Restaurant Tenant
        var restaurant = new Restaurant
        {
            Name = "The Urban Gourmet Bistro",
            Code = "DEMO-REST-01",
            ContactEmail = "admin@demo-bistro.com",
            ContactPhone = "+92 300 1234567",
            Status = TenantStatus.Active
        };
        _context.Restaurants.Add(restaurant);

        // 2. Create Primary Branch
        var newBranch = new Branch
        {
            RestaurantId = restaurant.Id,
            BranchName = "Downtown Flagship",
            BranchCode = "DT-01",
            Address = "Block 4, Clifton, Karachi",
            Phone = "+92 21 35876543",
            IsActive = true
        };
        _context.Branches.Add(newBranch);

        // 3. Create Admin User
        var adminUser = new User
        {
            RestaurantId = restaurant.Id,
            BranchId = newBranch.Id,
            Email = "admin@demo-bistro.com",
            NormalizedEmail = "ADMIN@DEMO-BISTRO.COM",
            FullName = "Saim Khan (GM)",
            PasswordHash = _hasher.HashPassword("DemoPass123!"),
            Status = UserStatus.Active
        };
        _context.Users.Add(adminUser);

        // 4. Floor Section & Tables
        var mainFloor = new FloorSection
        {
            RestaurantId = restaurant.Id,
            BranchId = newBranch.Id,
            Name = "Main Dining Area"
        };
        _context.FloorSections.Add(mainFloor);

        var tables = new List<RestaurantTable>();
        for (int i = 1; i <= 8; i++)
        {
            tables.Add(new RestaurantTable
            {
                RestaurantId = restaurant.Id,
                BranchId = newBranch.Id,
                FloorSectionId = mainFloor.Id,
                TableNumber = $"T-{i:D2}",
                Capacity = i % 2 == 0 ? 4 : 2,
                Status = TableStatus.Available
            });
        }
        _context.RestaurantTables.AddRange(tables);

        // 5. Menu Categories
        var catBurgers = new Category { RestaurantId = restaurant.Id, Name = "Gourmet Burgers", DisplayOrder = 1, IsActive = true };
        var catPizzas = new Category { RestaurantId = restaurant.Id, Name = "Artisan Pizzas", DisplayOrder = 2, IsActive = true };
        var catDrinks = new Category { RestaurantId = restaurant.Id, Name = "Mocktails & Shakes", DisplayOrder = 3, IsActive = true };
        _context.Categories.AddRange(catBurgers, catPizzas, catDrinks);

        // 6. Products
        var p1 = new Product
        {
            RestaurantId = restaurant.Id,
            CategoryId = catBurgers.Id,
            Name = "Smoky Angus Beef Burger",
            Description = "Prime Angus patty, smoked gouda, caramelized onions, brioche bun.",
            SKU = "BRG-ANG-01",
            BasePrice = 1450.00m,
            IsAvailable = true,
            ImageUrl = "https://images.unsplash.com/photo-1568901346375-23c9450c58cd?w=400"
        };
        var p2 = new Product
        {
            RestaurantId = restaurant.Id,
            CategoryId = catBurgers.Id,
            Name = "Crispy Buttermilk Chicken Burger",
            Description = "Fried spiced chicken thigh, chipotle slaw, pickles, signature aioli.",
            SKU = "BRG-CHK-02",
            BasePrice = 1150.00m,
            IsAvailable = true,
            ImageUrl = "https://images.unsplash.com/photo-1625813506062-0aeb1d7a094b?w=400"
        };
        var p3 = new Product
        {
            RestaurantId = restaurant.Id,
            CategoryId = catPizzas.Id,
            Name = "Truffle Margherita Pizza 12\"",
            Description = "San Marzano pomodoro, fresh buffalo mozzarella, basil, white truffle drizzle.",
            SKU = "PIZ-TRF-03",
            BasePrice = 1850.00m,
            IsAvailable = true,
            ImageUrl = "https://images.unsplash.com/photo-1604382354936-07c5d9983bd3?w=400"
        };
        var p4 = new Product
        {
            RestaurantId = restaurant.Id,
            CategoryId = catPizzas.Id,
            Name = "Spicy Pepperoni & Jalapeno",
            Description = "Double beef pepperoni, pickled jalapenos, hot honey drizzle, mozzarella.",
            SKU = "PIZ-PEP-04",
            BasePrice = 2100.00m,
            IsAvailable = true,
            ImageUrl = "https://images.unsplash.com/photo-1534308983496-4fabb1a015ee?w=400"
        };
        var p5 = new Product
        {
            RestaurantId = restaurant.Id,
            CategoryId = catDrinks.Id,
            Name = "Mint Lemonade Fizz",
            Description = "Fresh mint leaves, zesty lemon, crushed ice and sparkling soda.",
            SKU = "DRK-MNT-05",
            BasePrice = 450.00m,
            IsAvailable = true,
            ImageUrl = "https://images.unsplash.com/photo-1513558161293-cdaf765ed2fd?w=400"
        };
        var p6 = new Product
        {
            RestaurantId = restaurant.Id,
            CategoryId = catDrinks.Id,
            Name = "Belgian Chocolate Shake",
            Description = "Rich dark cocoa, vanilla gelato, whipped cream, chocolate shavings.",
            SKU = "DRK-CHK-06",
            BasePrice = 650.00m,
            IsAvailable = true,
            ImageUrl = "https://images.unsplash.com/photo-1572490122747-3968b75cc699?w=400"
        };

        _context.Products.AddRange(p1, p2, p3, p4, p5, p6);

        // 7. Add dynamic Tax Rules (e.g., 5% Card, 15% Cash)
        _context.TaxRules.Add(new TaxRule
        {
            RestaurantId = restaurant.Id,
            BranchId = newBranch.Id,
            Name = "Sales Tax (Card - 5%)",
            RatePercentage = 5.00m,
            AppliesToPaymentMethod = PaymentMethod.Card,
            IsActive = true
        });
        _context.TaxRules.Add(new TaxRule
        {
            RestaurantId = restaurant.Id,
            BranchId = newBranch.Id,
            Name = "Sales Tax (Cash - 15%)",
            RatePercentage = 15.00m,
            AppliesToPaymentMethod = PaymentMethod.Cash,
            IsActive = true
        });

        // 8. Add Demo Subscription Plan & Active Subscription
        var proPlan = new SubscriptionPlan
        {
            Name = "Pro",
            MonthlyPrice = 49.99m,
            MaxBranches = 5,
            MaxUsersPerBranch = 15,
            HasAdvancedAnalytics = true,
            HasFbrIntegration = true,
            IsActive = true
        };
        _context.SubscriptionPlans.Add(proPlan);

        var sub = new TenantSubscription
        {
            RestaurantId = restaurant.Id,
            PlanId = proPlan.Id,
            Status = SubscriptionStatus.Active,
            PeriodStart = DateTime.UtcNow,
            PeriodEnd = DateTime.UtcNow.AddMonths(1),
            AutoRenew = true
        };
        _context.TenantSubscriptions.Add(sub);

        await _context.SaveChangesAsync(cancellationToken);

        return new SeedDemoTenantResult(
            restaurant.Id,
            newBranch.Id,
            restaurant.Name,
            adminUser.Email,
            "DemoPass123!",
            3,
            6,
            tables.Count
        );
    }
}
