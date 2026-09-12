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
