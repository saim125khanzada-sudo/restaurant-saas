using Microsoft.EntityFrameworkCore;
using RestaurantSaaS.Domain.Entities;

namespace RestaurantSaaS.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Restaurant> Restaurants { get; }
    DbSet<Branch> Branches { get; }
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<RolePermission> RolePermissions { get; }
    DbSet<DeviceSession> DeviceSessions { get; }
    DbSet<AuditLog> AuditLogs { get; }

    // Catalog & Table Management
    DbSet<Category> Categories { get; }
    DbSet<Product> Products { get; }
    DbSet<ProductVariant> ProductVariants { get; }
    DbSet<Addon> Addons { get; }
    DbSet<ProductAddon> ProductAddons { get; }
    DbSet<FloorSection> FloorSections { get; }
    DbSet<RestaurantTable> RestaurantTables { get; }

    // Phase 3: Orders & POS Engine
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }
    DbSet<OrderItemAddon> OrderItemAddons { get; }
    DbSet<OrderStatusHistory> OrderStatusHistories { get; }
    DbSet<Payment> Payments { get; }

    // Phase 5: Delivery & Rider Management
    DbSet<DeliveryDispatch> DeliveryDispatches { get; }
    DbSet<RiderLocationHistory> RiderLocationHistories { get; }
    DbSet<RiderCashReconciliation> RiderCashReconciliations { get; }

    // Phase 6: Inventory & Procurement
    DbSet<Vendor> Vendors { get; }
    DbSet<Ingredient> Ingredients { get; }
    DbSet<RecipeItem> RecipeItems { get; }
    DbSet<StockLevel> StockLevels { get; }
    DbSet<StockMovement> StockMovements { get; }
    DbSet<PurchaseOrder> PurchaseOrders { get; }
    DbSet<PurchaseOrderItem> PurchaseOrderItems { get; }

    // Phase 7: Double-Entry Accounting
    DbSet<Account> Accounts { get; }
    DbSet<JournalEntry> JournalEntries { get; }
    DbSet<JournalLine> JournalLines { get; }
    DbSet<CashRegisterSession> CashRegisterSessions { get; }

    // Phase 8: HR & Payroll
    DbSet<Employee> Employees { get; }
    DbSet<AttendanceRecord> AttendanceRecords { get; }
    DbSet<StaffAdvance> StaffAdvances { get; }
    DbSet<PayrollRun> PayrollRuns { get; }
    DbSet<PayrollDetail> PayrollDetails { get; }

    // Phase 9: Taxation & Fiscalization
    DbSet<TaxRule> TaxRules { get; }
    DbSet<FiscalInvoiceRecord> FiscalInvoiceRecords { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface ICurrentTenantService
{
    Guid? RestaurantId { get; }
    Guid? BranchId { get; }
    Guid? UserId { get; }
    bool IsSuperAdmin { get; }
    void SetTenant(Guid restaurantId, Guid? branchId = null);
}

public interface IJwtTokenService
{
    string GenerateAccessToken(User user, IEnumerable<string> roles, IEnumerable<string> permissions);
    string GenerateRefreshToken();
    string HashToken(string token);
}

public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash);
}

public interface IMfaService
{
    string GenerateSecret();
    string GenerateQrCodeUri(string email, string secret, string issuer = "RestaurantSaaS");
    bool VerifyTotp(string secret, string code);
}
