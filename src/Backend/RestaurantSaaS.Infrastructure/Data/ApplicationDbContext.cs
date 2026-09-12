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

        // GLOBAL MULTI-TENANCY QUERY FILTERS
        // When authenticated as a tenant user, EF Core automatically appends WHERE RestaurantId = @CurrentTenant
        // When user is SuperAdmin or TenantId is null, filter is bypassed.
        modelBuilder.Entity<Branch>().HasQueryFilter(e => !_tenantService.RestaurantId.HasValue || e.RestaurantId == _tenantService.RestaurantId.Value);
        modelBuilder.Entity<User>().HasQueryFilter(e => !_tenantService.RestaurantId.HasValue || e.RestaurantId == _tenantService.RestaurantId.Value);
        modelBuilder.Entity<Role>().HasQueryFilter(e => !_tenantService.RestaurantId.HasValue || e.RestaurantId == _tenantService.RestaurantId.Value);
        modelBuilder.Entity<UserRole>().HasQueryFilter(e => !_tenantService.RestaurantId.HasValue || e.RestaurantId == _tenantService.RestaurantId.Value);
        modelBuilder.Entity<RolePermission>().HasQueryFilter(e => !_tenantService.RestaurantId.HasValue || e.RestaurantId == _tenantService.RestaurantId.Value);
        modelBuilder.Entity<DeviceSession>().HasQueryFilter(e => !_tenantService.RestaurantId.HasValue || e.RestaurantId == _tenantService.RestaurantId.Value);
        modelBuilder.Entity<AuditLog>().HasQueryFilter(e => !_tenantService.RestaurantId.HasValue || e.RestaurantId == _tenantService.RestaurantId.Value);
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

        // Automatic RestaurantId injection for added multi-tenant entities if not already specified
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
