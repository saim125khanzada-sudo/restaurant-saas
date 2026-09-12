using Microsoft.EntityFrameworkCore;
using RestaurantSaaS.Application.Interfaces;
using RestaurantSaaS.Domain.Entities;
using RestaurantSaaS.SharedKernel.Common;
using RestaurantSaaS.SharedKernel.Interfaces;

namespace RestaurantSaaS.Infrastructure.Data;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly ICurrentTenantService _tenantService;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentTenantService tenantService) : base(options)
    {
        _tenantService = tenantService;
    }

    public DbSet<Restaurant> Restaurants => Set<Restaurant>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<DeviceSession> DeviceSessions => Set<DeviceSession>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    // Phase 2: Catalog & Facilities
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<Addon> Addons => Set<Addon>();
    public DbSet<ProductAddon> ProductAddons => Set<ProductAddon>();
    public DbSet<FloorSection> FloorSections => Set<FloorSection>();
    public DbSet<RestaurantTable> RestaurantTables => Set<RestaurantTable>();

    // Phase 3: Orders & POS Engine
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderItemAddon> OrderItemAddons => Set<OrderItemAddon>();
    public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();
    public DbSet<Payment> Payments => Set<Payment>();

    // Phase 5: Delivery & Rider Management
    public DbSet<DeliveryDispatch> DeliveryDispatches => Set<DeliveryDispatch>();
    public DbSet<RiderLocationHistory> RiderLocationHistories => Set<RiderLocationHistory>();
    public DbSet<RiderCashReconciliation> RiderCashReconciliations => Set<RiderCashReconciliation>();

    // Phase 6: Inventory & Procurement
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<Ingredient> Ingredients => Set<Ingredient>();
    public DbSet<RecipeItem> RecipeItems => Set<RecipeItem>();
    public DbSet<StockLevel> StockLevels => Set<StockLevel>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderItem> PurchaseOrderItems => Set<PurchaseOrderItem>();

    // Phase 7: Double-Entry Accounting
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
    public DbSet<JournalLine> JournalLines => Set<JournalLine>();
    public DbSet<CashRegisterSession> CashRegisterSessions => Set<CashRegisterSession>();

    // Phase 8: HR & Payroll
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
    public DbSet<StaffAdvance> StaffAdvances => Set<StaffAdvance>();
    public DbSet<PayrollRun> PayrollRuns => Set<PayrollRun>();
    public DbSet<PayrollDetail> PayrollDetails => Set<PayrollDetail>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Restaurants
        modelBuilder.Entity<Restaurant>(b =>
        {
            b.ToTable("restaurants");
            b.HasKey(r => r.Id);
            b.HasIndex(r => r.Code).IsUnique();
            b.Property(r => r.Name).HasMaxLength(150).IsRequired();
            b.Property(r => r.Code).HasMaxLength(50).IsRequired();
        });

        // Branches
        modelBuilder.Entity<Branch>(b =>
        {
            b.ToTable("branches");
            b.HasKey(br => br.Id);
            b.HasIndex(br => new { br.RestaurantId, br.BranchCode }).IsUnique();
            b.Property(br => br.BranchName).HasMaxLength(150).IsRequired();
            b.HasOne(br => br.Restaurant)
                .WithMany(r => r.Branches)
                .HasForeignKey(br => br.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Users
        modelBuilder.Entity<User>(b =>
        {
            b.ToTable("users");
            b.HasKey(u => u.Id);
            b.HasIndex(u => new { u.RestaurantId, u.NormalizedEmail }).IsUnique();
            b.Property(u => u.Email).HasMaxLength(256).IsRequired();
            b.Property(u => u.NormalizedEmail).HasMaxLength(256).IsRequired();
            b.Property(u => u.FullName).HasMaxLength(150).IsRequired();
            b.HasOne(u => u.Restaurant)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(u => u.Branch)
                .WithMany()
                .HasForeignKey(u => u.BranchId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Roles & Permissions
        modelBuilder.Entity<Role>(b =>
        {
            b.ToTable("roles");
            b.HasKey(r => r.Id);
            b.HasIndex(r => new { r.RestaurantId, r.Name }).IsUnique();
            b.Property(r => r.Name).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<Permission>(b =>
        {
            b.ToTable("permissions");
            b.HasKey(p => p.Id);
            b.HasIndex(p => p.Code).IsUnique();
            b.Property(p => p.Code).HasMaxLength(100).IsRequired();
            b.Property(p => p.Name).HasMaxLength(150).IsRequired();
        });

        modelBuilder.Entity<UserRole>(b =>
        {
            b.ToTable("user_roles");
            b.HasKey(ur => ur.Id);
            b.HasIndex(ur => new { ur.UserId, ur.RoleId }).IsUnique();
        });

        modelBuilder.Entity<RolePermission>(b =>
        {
            b.ToTable("role_permissions");
            b.HasKey(rp => rp.Id);
            b.HasIndex(rp => new { rp.RoleId, rp.PermissionId }).IsUnique();
        });

        modelBuilder.Entity<DeviceSession>(b =>
        {
            b.ToTable("device_sessions");
            b.HasKey(s => s.Id);
            b.HasIndex(s => s.RefreshTokenHash);
        });

        modelBuilder.Entity<AuditLog>(b =>
        {
            b.ToTable("audit_logs");
            b.HasKey(a => a.Id);
            b.HasIndex(a => new { a.RestaurantId, a.CreatedAt });
        });

        // Phase 2: Catalog & Facilities
        modelBuilder.Entity<Category>(b =>
        {
            b.ToTable("categories");
            b.HasKey(c => c.Id);
            b.HasIndex(c => new { c.RestaurantId, c.DisplayOrder });
            b.Property(c => c.Name).HasMaxLength(100).IsRequired();
            b.HasOne(c => c.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Product>(b =>
        {
            b.ToTable("products");
            b.HasKey(p => p.Id);
            b.HasIndex(p => new { p.RestaurantId, p.SKU }).IsUnique();
            b.HasIndex(p => new { p.RestaurantId, p.CategoryId, p.IsAvailable });
            b.Property(p => p.Name).HasMaxLength(150).IsRequired();
            b.Property(p => p.SKU).HasMaxLength(50).IsRequired();
            b.Property(p => p.BasePrice).HasPrecision(18, 2);
            b.Property(p => p.CostPrice).HasPrecision(18, 2);
            b.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProductVariant>(b =>
        {
            b.ToTable("product_variants");
            b.HasKey(v => v.Id);
            b.Property(v => v.Name).HasMaxLength(100).IsRequired();
            b.Property(v => v.Price).HasPrecision(18, 2);
            b.Property(v => v.CostPrice).HasPrecision(18, 2);
            b.HasOne(v => v.Product)
                .WithMany(p => p.Variants)
                .HasForeignKey(v => v.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Addon>(b =>
        {
            b.ToTable("addons");
            b.HasKey(a => a.Id);
            b.Property(a => a.Name).HasMaxLength(100).IsRequired();
            b.Property(a => a.Price).HasPrecision(18, 2);
            b.Property(a => a.CostPrice).HasPrecision(18, 2);
        });

        modelBuilder.Entity<ProductAddon>(b =>
        {
            b.ToTable("product_addons");
            b.HasKey(pa => pa.Id);
            b.HasIndex(pa => new { pa.ProductId, pa.AddonId }).IsUnique();
            b.HasOne(pa => pa.Product)
                .WithMany(p => p.ProductAddons)
                .HasForeignKey(pa => pa.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(pa => pa.Addon)
                .WithMany(a => a.ProductAddons)
                .HasForeignKey(pa => pa.AddonId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<FloorSection>(b =>
        {
            b.ToTable("floor_sections");
            b.HasKey(fs => fs.Id);
            b.Property(fs => fs.Name).HasMaxLength(100).IsRequired();
            b.HasOne(fs => fs.Branch)
                .WithMany()
                .HasForeignKey(fs => fs.BranchId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RestaurantTable>(b =>
        {
            b.ToTable("restaurant_tables");
            b.HasKey(t => t.Id);
            b.HasIndex(t => new { t.RestaurantId, t.BranchId, t.TableNumber }).IsUnique();
            b.Property(t => t.TableNumber).HasMaxLength(50).IsRequired();
            b.HasOne(t => t.Branch)
                .WithMany()
                .HasForeignKey(t => t.BranchId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(t => t.FloorSection)
                .WithMany(fs => fs.Tables)
                .HasForeignKey(t => t.FloorSectionId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Phase 3: Orders, Items, Payments & History
        modelBuilder.Entity<Order>(b =>
        {
            b.ToTable("orders");
            b.HasKey(o => o.Id);
            b.HasIndex(o => new { o.RestaurantId, o.OrderNumber }).IsUnique();
            b.HasIndex(o => new { o.RestaurantId, o.CreatedAt });
            b.HasIndex(o => new { o.RestaurantId, o.Status });
            b.HasIndex(o => o.IdempotencyKey);
            b.Property(o => o.OrderNumber).HasMaxLength(50).IsRequired();
            b.Property(o => o.Subtotal).HasPrecision(18, 2);
            b.Property(o => o.TaxTotal).HasPrecision(18, 2);
            b.Property(o => o.DiscountTotal).HasPrecision(18, 2);
            b.Property(o => o.GrandTotal).HasPrecision(18, 2);
            b.HasOne(o => o.Branch)
                .WithMany()
                .HasForeignKey(o => o.BranchId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(o => o.Table)
                .WithMany()
                .HasForeignKey(o => o.TableId)
                .OnDelete(DeleteBehavior.SetNull);
            b.HasOne(o => o.Waiter)
                .WithMany()
                .HasForeignKey(o => o.WaiterId)
                .OnDelete(DeleteBehavior.SetNull);
            b.HasOne(o => o.Rider)
                .WithMany()
                .HasForeignKey(o => o.RiderId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<OrderItem>(b =>
        {
            b.ToTable("order_items");
            b.HasKey(oi => oi.Id);
            b.HasIndex(oi => oi.OrderId);
            b.Property(oi => oi.ItemName).HasMaxLength(150).IsRequired();
            b.Property(oi => oi.UnitPrice).HasPrecision(18, 2);
            b.Property(oi => oi.TotalPrice).HasPrecision(18, 2);
            b.HasOne(oi => oi.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderItemAddon>(b =>
        {
            b.ToTable("order_item_addons");
            b.HasKey(oia => oia.Id);
            b.Property(oia => oia.AddonName).HasMaxLength(100).IsRequired();
            b.Property(oia => oia.UnitPrice).HasPrecision(18, 2);
            b.Property(oia => oia.TotalPrice).HasPrecision(18, 2);
            b.HasOne(oia => oia.OrderItem)
                .WithMany(oi => oi.Addons)
                .HasForeignKey(oia => oia.OrderItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderStatusHistory>(b =>
        {
            b.ToTable("order_status_history");
            b.HasKey(h => h.Id);
            b.HasIndex(h => new { h.OrderId, h.CreatedAt });
            b.HasOne(h => h.Order)
                .WithMany(o => o.StatusHistory)
                .HasForeignKey(h => h.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Payment>(b =>
        {
            b.ToTable("payments");
            b.HasKey(p => p.Id);
            b.HasIndex(p => new { p.RestaurantId, p.ProcessedAt });
            b.Property(p => p.Amount).HasPrecision(18, 2);
            b.HasOne(p => p.Order)
                .WithMany(o => o.Payments)
                .HasForeignKey(p => p.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Phase 5: Delivery & Rider Mapping
        modelBuilder.Entity<DeliveryDispatch>(b =>
        {
            b.ToTable("delivery_dispatches");
            b.HasKey(x => x.Id);
            b.Property(x => x.DeliveryAddress).HasMaxLength(500).IsRequired();
            b.Property(x => x.CustomerPhone).HasMaxLength(30);
            b.Property(x => x.CustomerName).HasMaxLength(150);
            b.Property(x => x.DestinationLatitude).HasPrecision(10, 7);
            b.Property(x => x.DestinationLongitude).HasPrecision(10, 7);
            b.Property(x => x.CashToCollect).HasPrecision(18, 2);
            b.Property(x => x.CashCollected).HasPrecision(18, 2);
            b.Property(x => x.FailureReason).HasMaxLength(250);
            b.HasOne(x => x.Order).WithMany().HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.Rider).WithMany().HasForeignKey(x => x.RiderId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => new { x.RestaurantId, x.BranchId, x.Status });
            b.HasIndex(x => new { x.RiderId, x.Status });
        });

        modelBuilder.Entity<RiderLocationHistory>(b =>
        {
            b.ToTable("rider_location_history");
            b.HasKey(x => x.Id);
            b.Property(x => x.Latitude).HasPrecision(10, 7).IsRequired();
            b.Property(x => x.Longitude).HasPrecision(10, 7).IsRequired();
            b.Property(x => x.Heading).HasPrecision(5, 2);
            b.Property(x => x.SpeedKmh).HasPrecision(6, 2);
            b.HasIndex(x => new { x.RiderId, x.RecordedAt });
            b.HasIndex(x => new { x.RestaurantId, x.RecordedAt });
        });

        modelBuilder.Entity<RiderCashReconciliation>(b =>
        {
            b.ToTable("rider_cash_reconciliations");
            b.HasKey(x => x.Id);
            b.Property(x => x.TotalCashExpected).HasPrecision(18, 2);
            b.Property(x => x.TotalCashSubmitted).HasPrecision(18, 2);
            b.Property(x => x.DiscrepancyAmount).HasPrecision(18, 2);
            b.Property(x => x.Notes).HasMaxLength(500);
            b.HasOne(x => x.Rider).WithMany().HasForeignKey(x => x.RiderId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.ApprovedByUser).WithMany().HasForeignKey(x => x.ApprovedByUserId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => new { x.RestaurantId, x.BranchId, x.ShiftDate });
            b.HasIndex(x => new { x.RiderId, x.ShiftDate });
        });

        // Phase 6: Inventory & Procurement Mapping
        modelBuilder.Entity<Vendor>(b =>
        {
            b.ToTable("vendors");
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).HasMaxLength(150).IsRequired();
            b.Property(x => x.ContactPerson).HasMaxLength(100);
            b.Property(x => x.Phone).HasMaxLength(30);
            b.Property(x => x.Email).HasMaxLength(100);
            b.Property(x => x.TaxNumber).HasMaxLength(50);
            b.HasIndex(x => new { x.RestaurantId, x.Name });
        });

        modelBuilder.Entity<Ingredient>(b =>
        {
            b.ToTable("ingredients");
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).HasMaxLength(150).IsRequired();
            b.Property(x => x.UnitOfMeasure).HasMaxLength(20).IsRequired();
            b.Property(x => x.ReorderThreshold).HasPrecision(12, 3);
            b.Property(x => x.CostPerUnit).HasPrecision(18, 2);
            b.HasIndex(x => new { x.RestaurantId, x.Name });
        });

        modelBuilder.Entity<RecipeItem>(b =>
        {
            b.ToTable("recipe_items");
            b.HasKey(x => x.Id);
            b.Property(x => x.QuantityRequired).HasPrecision(12, 3);
            b.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.ProductVariant).WithMany().HasForeignKey(x => x.ProductVariantId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.Ingredient).WithMany().HasForeignKey(x => x.IngredientId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => new { x.ProductId, x.ProductVariantId, x.IngredientId });
        });

        modelBuilder.Entity<StockLevel>(b =>
        {
            b.ToTable("stock_levels");
            b.HasKey(x => x.Id);
            b.Property(x => x.QuantityOnHand).HasPrecision(12, 3);
            b.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.Ingredient).WithMany().HasForeignKey(x => x.IngredientId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => new { x.BranchId, x.IngredientId }).IsUnique();
        });

        modelBuilder.Entity<StockMovement>(b =>
        {
            b.ToTable("stock_movements");
            b.HasKey(x => x.Id);
            b.Property(x => x.Quantity).HasPrecision(12, 3);
            b.Property(x => x.UnitCost).HasPrecision(18, 2);
            b.Property(x => x.Reason).HasMaxLength(250);
            b.HasOne(x => x.Ingredient).WithMany().HasForeignKey(x => x.IngredientId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => new { x.RestaurantId, x.BranchId, x.IngredientId, x.CreatedAt });
        });

        modelBuilder.Entity<PurchaseOrder>(b =>
        {
            b.ToTable("purchase_orders");
            b.HasKey(x => x.Id);
            b.Property(x => x.PoNumber).HasMaxLength(50).IsRequired();
            b.Property(x => x.TotalAmount).HasPrecision(18, 2);
            b.Property(x => x.Notes).HasMaxLength(500);
            b.HasOne(x => x.Vendor).WithMany().HasForeignKey(x => x.VendorId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => new { x.RestaurantId, x.PoNumber }).IsUnique();
        });

        modelBuilder.Entity<PurchaseOrderItem>(b =>
        {
            b.ToTable("purchase_order_items");
            b.HasKey(x => x.Id);
            b.Property(x => x.QuantityOrdered).HasPrecision(12, 3);
            b.Property(x => x.QuantityReceived).HasPrecision(12, 3);
            b.Property(x => x.UnitPrice).HasPrecision(18, 2);
            b.Property(x => x.TotalPrice).HasPrecision(18, 2);
            b.HasOne(x => x.Ingredient).WithMany().HasForeignKey(x => x.IngredientId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<PurchaseOrder>().WithMany(p => p.Items).HasForeignKey(x => x.PurchaseOrderId).OnDelete(DeleteBehavior.Cascade);
        });

        // Phase 7: Double-Entry Accounting Mapping
        modelBuilder.Entity<Account>(b =>
        {
            b.ToTable("accounts");
            b.HasKey(x => x.Id);
            b.Property(x => x.AccountCode).HasMaxLength(30).IsRequired();
            b.Property(x => x.Name).HasMaxLength(150).IsRequired();
            b.Property(x => x.Currency).HasMaxLength(10).IsRequired();
            b.Property(x => x.Description).HasMaxLength(250);
            b.HasIndex(x => new { x.RestaurantId, x.AccountCode }).IsUnique();
        });

        modelBuilder.Entity<JournalEntry>(b =>
        {
            b.ToTable("journal_entries");
            b.HasKey(x => x.Id);
            b.Property(x => x.EntryNumber).HasMaxLength(50).IsRequired();
            b.Property(x => x.Description).HasMaxLength(500).IsRequired();
            b.Property(x => x.TotalDebit).HasPrecision(18, 2);
            b.Property(x => x.TotalCredit).HasPrecision(18, 2);
            b.Property(x => x.SourceDocumentType).HasMaxLength(50);
            b.HasIndex(x => new { x.RestaurantId, x.EntryNumber }).IsUnique();
            b.HasIndex(x => new { x.RestaurantId, x.PostingDate });
        });

        modelBuilder.Entity<JournalLine>(b =>
        {
            b.ToTable("journal_lines");
            b.HasKey(x => x.Id);
            b.Property(x => x.DebitAmount).HasPrecision(18, 2);
            b.Property(x => x.CreditAmount).HasPrecision(18, 2);
            b.Property(x => x.LineDescription).HasMaxLength(250);
            b.HasOne(x => x.Account).WithMany().HasForeignKey(x => x.AccountId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<JournalEntry>().WithMany(j => j.Lines).HasForeignKey(x => x.JournalEntryId).OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(x => new { x.JournalEntryId, x.AccountId });
        });

        modelBuilder.Entity<CashRegisterSession>(b =>
        {
            b.ToTable("cash_register_sessions");
            b.HasKey(x => x.Id);
            b.Property(x => x.OpeningFloat).HasPrecision(18, 2);
            b.Property(x => x.CashSales).HasPrecision(18, 2);
            b.Property(x => x.CashDrops).HasPrecision(18, 2);
            b.Property(x => x.ExpectedCash).HasPrecision(18, 2);
            b.Property(x => x.ActualCountedCash).HasPrecision(18, 2);
            b.Property(x => x.Discrepancy).HasPrecision(18, 2);
            b.Property(x => x.Notes).HasMaxLength(500);
            b.HasOne(x => x.CashierUser).WithMany().HasForeignKey(x => x.CashierUserId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => new { x.RestaurantId, x.BranchId, x.OpenedAt });
        });

        // Phase 8: HR & Payroll Mapping
        modelBuilder.Entity<Employee>(b =>
        {
            b.ToTable("employees");
            b.HasKey(x => x.Id);
            b.Property(x => x.EmployeeCode).HasMaxLength(30).IsRequired();
            b.Property(x => x.FullName).HasMaxLength(150).IsRequired();
            b.Property(x => x.Designation).HasMaxLength(100).IsRequired();
            b.Property(x => x.Phone).HasMaxLength(30);
            b.Property(x => x.Email).HasMaxLength(100);
            b.Property(x => x.NationalId).HasMaxLength(50);
            b.Property(x => x.BiometricUserId).HasMaxLength(50);
            b.Property(x => x.BaseMonthlySalary).HasPrecision(18, 2);
            b.Property(x => x.HourlyOvertimeRate).HasPrecision(18, 2);
            b.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.SetNull);
            b.HasIndex(x => new { x.RestaurantId, x.EmployeeCode }).IsUnique();
        });

        modelBuilder.Entity<AttendanceRecord>(b =>
        {
            b.ToTable("attendance_records");
            b.HasKey(x => x.Id);
            b.Property(x => x.TotalHoursWorked).HasPrecision(5, 2);
            b.Property(x => x.OvertimeHours).HasPrecision(5, 2);
            b.Property(x => x.DeviceIdentifier).HasMaxLength(50);
            b.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(x => new { x.RestaurantId, x.BranchId, x.WorkDate });
            b.HasIndex(x => new { x.EmployeeId, x.WorkDate });
        });

        modelBuilder.Entity<StaffAdvance>(b =>
        {
            b.ToTable("staff_advances");
            b.HasKey(x => x.Id);
            b.Property(x => x.PrincipalAmount).HasPrecision(18, 2);
            b.Property(x => x.MonthlyDeductionAmount).HasPrecision(18, 2);
            b.Property(x => x.RemainingBalance).HasPrecision(18, 2);
            b.Property(x => x.Purpose).HasMaxLength(250);
            b.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(x => new { x.RestaurantId, x.EmployeeId });
        });

        modelBuilder.Entity<PayrollRun>(b =>
        {
            b.ToTable("payroll_runs");
            b.HasKey(x => x.Id);
            b.Property(x => x.TotalGrossPay).HasPrecision(18, 2);
            b.Property(x => x.TotalDeductions).HasPrecision(18, 2);
            b.Property(x => x.TotalNetPay).HasPrecision(18, 2);
            b.HasIndex(x => new { x.RestaurantId, x.BranchId, x.Year, x.Month }).IsUnique();
        });

        modelBuilder.Entity<PayrollDetail>(b =>
        {
            b.ToTable("payroll_details");
            b.HasKey(x => x.Id);
            b.Property(x => x.BaseSalary).HasPrecision(18, 2);
            b.Property(x => x.OvertimePay).HasPrecision(18, 2);
            b.Property(x => x.AdvanceDeduction).HasPrecision(18, 2);
            b.Property(x => x.OtherDeductions).HasPrecision(18, 2);
            b.Property(x => x.NetSalaryPayable).HasPrecision(18, 2);
            b.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<PayrollRun>().WithMany(p => p.Details).HasForeignKey(x => x.PayrollRunId).OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(x => new { x.PayrollRunId, x.EmployeeId });
        });

        // GLOBAL MULTI-TENANCY QUERY FILTERS
        modelBuilder.Entity<Branch>().HasQueryFilter(e => !_tenantService.RestaurantId.HasValue || e.RestaurantId == _tenantService.RestaurantId.Value);
        modelBuilder.Entity<User>().HasQueryFilter(e => !_tenantService.RestaurantId.HasValue || e.RestaurantId == _tenantService.RestaurantId.Value);
        modelBuilder.Entity<Role>().HasQueryFilter(e => !_tenantService.RestaurantId.HasValue || e.RestaurantId == _tenantService.RestaurantId.Value);
        modelBuilder.Entity<UserRole>().HasQueryFilter(e => !_tenantService.RestaurantId.HasValue || e.RestaurantId == _tenantService.RestaurantId.Value);
        modelBuilder.Entity<RolePermission>().HasQueryFilter(e => !_tenantService.RestaurantId.HasValue || e.RestaurantId == _tenantService.RestaurantId.Value);
        modelBuilder.Entity<DeviceSession>().HasQueryFilter(e => !_tenantService.RestaurantId.HasValue || e.RestaurantId == _tenantService.RestaurantId.Value);
        modelBuilder.Entity<AuditLog>().HasQueryFilter(e => !_tenantService.RestaurantId.HasValue || e.RestaurantId == _tenantService.RestaurantId.Value);

        modelBuilder.Entity<Category>().HasQueryFilter(e => !_tenantService.RestaurantId.HasValue || e.RestaurantId == _tenantService.RestaurantId.Value);
        modelBuilder.Entity<Product>().HasQueryFilter(e => !_tenantService.RestaurantId.HasValue || e.RestaurantId == _tenantService.RestaurantId.Value);
        modelBuilder.Entity<ProductVariant>().HasQueryFilter(e => !_tenantService.RestaurantId.HasValue || e.RestaurantId == _tenantService.RestaurantId.Value);
        modelBuilder.Entity<Addon>().HasQueryFilter(e => !_tenantService.RestaurantId.HasValue || e.RestaurantId == _tenantService.RestaurantId.Value);
        modelBuilder.Entity<ProductAddon>().HasQueryFilter(e => !_tenantService.RestaurantId.HasValue || e.RestaurantId == _tenantService.RestaurantId.Value);
        modelBuilder.Entity<FloorSection>().HasQueryFilter(e => !_tenantService.RestaurantId.HasValue || e.RestaurantId == _tenantService.RestaurantId.Value);
        modelBuilder.Entity<RestaurantTable>().HasQueryFilter(e => !_tenantService.RestaurantId.HasValue || e.RestaurantId == _tenantService.RestaurantId.Value);

        modelBuilder.Entity<Order>().HasQueryFilter(e => !_tenantService.RestaurantId.HasValue || e.RestaurantId == _tenantService.RestaurantId.Value);
        modelBuilder.Entity<OrderItem>().HasQueryFilter(e => !_tenantService.RestaurantId.HasValue || e.RestaurantId == _tenantService.RestaurantId.Value);
        modelBuilder.Entity<OrderItemAddon>().HasQueryFilter(e => !_tenantService.RestaurantId.HasValue || e.RestaurantId == _tenantService.RestaurantId.Value);
        modelBuilder.Entity<OrderStatusHistory>().HasQueryFilter(e => !_tenantService.RestaurantId.HasValue || e.RestaurantId == _tenantService.RestaurantId.Value);
        modelBuilder.Entity<Payment>().HasQueryFilter(e => !_tenantService.RestaurantId.HasValue || e.RestaurantId == _tenantService.RestaurantId.Value);

        modelBuilder.Entity<DeliveryDispatch>().HasQueryFilter(e => !_tenantService.RestaurantId.HasValue || e.RestaurantId == _tenantService.RestaurantId.Value);
        modelBuilder.Entity<RiderLocationHistory>().HasQueryFilter(e => !_tenantService.RestaurantId.HasValue || e.RestaurantId == _tenantService.RestaurantId.Value);
        modelBuilder.Entity<RiderCashReconciliation>().HasQueryFilter(e => !_tenantService.RestaurantId.HasValue || e.RestaurantId == _tenantService.RestaurantId.Value);

        modelBuilder.Entity<Vendor>().HasQueryFilter(e => !_tenantService.RestaurantId.HasValue || e.RestaurantId == _tenantService.RestaurantId.Value);
        modelBuilder.Entity<Ingredient>().HasQueryFilter(e => !_tenantService.RestaurantId.HasValue || e.RestaurantId == _tenantService.RestaurantId.Value);
        modelBuilder.Entity<RecipeItem>().HasQueryFilter(e => !_tenantService.RestaurantId.HasValue || e.RestaurantId == _tenantService.RestaurantId.Value);
        modelBuilder.Entity<StockLevel>().HasQueryFilter(e => !_tenantService.RestaurantId.HasValue || e.RestaurantId == _tenantService.RestaurantId.Value);
        modelBuilder.Entity<StockMovement>().HasQueryFilter(e => !_tenantService.RestaurantId.HasValue || e.RestaurantId == _tenantService.RestaurantId.Value);
        modelBuilder.Entity<PurchaseOrder>().HasQueryFilter(e => !_tenantService.RestaurantId.HasValue || e.RestaurantId == _tenantService.RestaurantId.Value);
        modelBuilder.Entity<PurchaseOrderItem>().HasQueryFilter(e => !_tenantService.RestaurantId.HasValue || e.RestaurantId == _tenantService.RestaurantId.Value);

        modelBuilder.Entity<Account>().HasQueryFilter(e => !_tenantService.RestaurantId.HasValue || e.RestaurantId == _tenantService.RestaurantId.Value);
        modelBuilder.Entity<JournalEntry>().HasQueryFilter(e => !_tenantService.RestaurantId.HasValue || e.RestaurantId == _tenantService.RestaurantId.Value);
        modelBuilder.Entity<JournalLine>().HasQueryFilter(e => !_tenantService.RestaurantId.HasValue || e.RestaurantId == _tenantService.RestaurantId.Value);
        modelBuilder.Entity<CashRegisterSession>().HasQueryFilter(e => !_tenantService.RestaurantId.HasValue || e.RestaurantId == _tenantService.RestaurantId.Value);

        modelBuilder.Entity<Employee>().HasQueryFilter(e => !_tenantService.RestaurantId.HasValue || e.RestaurantId == _tenantService.RestaurantId.Value);
        modelBuilder.Entity<AttendanceRecord>().HasQueryFilter(e => !_tenantService.RestaurantId.HasValue || e.RestaurantId == _tenantService.RestaurantId.Value);
        modelBuilder.Entity<StaffAdvance>().HasQueryFilter(e => !_tenantService.RestaurantId.HasValue || e.RestaurantId == _tenantService.RestaurantId.Value);
        modelBuilder.Entity<PayrollRun>().HasQueryFilter(e => !_tenantService.RestaurantId.HasValue || e.RestaurantId == _tenantService.RestaurantId.Value);
        modelBuilder.Entity<PayrollDetail>().HasQueryFilter(e => !_tenantService.RestaurantId.HasValue || e.RestaurantId == _tenantService.RestaurantId.Value);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTimeOffset.UtcNow;
                if (string.IsNullOrWhiteSpace(entry.Entity.CreatedBy))
                {
                    entry.Entity.CreatedBy = _tenantService.UserId?.ToString() ?? "SYSTEM";
                }
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTimeOffset.UtcNow;
                entry.Entity.UpdatedBy = _tenantService.UserId?.ToString() ?? "SYSTEM";
            }
        }

        foreach (var entry in ChangeTracker.Entries<IMultiTenantEntity>())
        {
            if (entry.State == EntityState.Added && entry.Entity.RestaurantId == Guid.Empty && _tenantService.RestaurantId.HasValue)
            {
                entry.Entity.RestaurantId = _tenantService.RestaurantId.Value;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
