using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RestaurantSaaS.Application.Interfaces;
using RestaurantSaaS.Domain.Entities;

namespace RestaurantSaaS.Application.Security;

public interface IAuditLogService
{
    Task<AuditLog> RecordAuditAsync(
        Guid restaurantId,
        Guid? branchId,
        Guid? userId,
        string action,
        string entityName,
        string? entityId,
        string? oldValuesJson,
        string? newValuesJson,
        string ipAddress,
        string userAgent,
        CancellationToken cancellationToken = default);

    Task<bool> VerifyAuditChainIntegrityAsync(Guid restaurantId, CancellationToken cancellationToken = default);
}

public class AuditLogService : IAuditLogService
{
    private readonly IApplicationDbContext _context;

    public AuditLogService(IApplicationDbContext context)
    {
        _context = context;
    }

    public static string ComputeHash(
        Guid restaurantId,
        string action,
        string entityName,
        string? entityId,
        string? oldValues,
        string? newValues,
        DateTimeOffset timestamp,
        string previousHash)
    {
        var rawData = $"{restaurantId}:{action}:{entityName}:{entityId}:{oldValues}:{newValues}:{timestamp:O}:{previousHash}";
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public async Task<AuditLog> RecordAuditAsync(
        Guid restaurantId,
        Guid? branchId,
        Guid? userId,
        string action,
        string entityName,
        string? entityId,
        string? oldValuesJson,
        string? newValuesJson,
        string ipAddress,
        string userAgent,
        CancellationToken cancellationToken = default)
    {
        // 1. Fetch latest audit log for this restaurant to chain hash
        var lastLog = await _context.AuditLogs
            .Where(a => a.RestaurantId == restaurantId)
            .OrderByDescending(a => a.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var prevHash = lastLog?.CurrentHash ?? "0000000000000000000000000000000000000000000000000000000000000000";
        var now = DateTimeOffset.UtcNow;
        var currentHash = ComputeHash(restaurantId, action, entityName, entityId, oldValuesJson, newValuesJson, now, prevHash);

        var auditLog = new AuditLog
        {
            RestaurantId = restaurantId,
            BranchId = branchId,
            UserId = userId,
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            OldValuesJson = oldValuesJson,
            NewValuesJson = newValuesJson,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            PreviousHash = prevHash,
            CurrentHash = currentHash,
            CreatedAt = now
        };

        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync(cancellationToken);

        return auditLog;
    }

    public async Task<bool> VerifyAuditChainIntegrityAsync(Guid restaurantId, CancellationToken cancellationToken = default)
    {
        var logs = await _context.AuditLogs
            .Where(a => a.RestaurantId == restaurantId)
            .OrderBy(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

        if (logs.Count == 0) return true;

        var expectedPrev = "0000000000000000000000000000000000000000000000000000000000000000";
        foreach (var log in logs)
        {
            if (log.PreviousHash != expectedPrev)
            {
                return false; // Broken link in chain
            }

            var calculatedHash = ComputeHash(
                log.RestaurantId,
                log.Action,
                log.EntityName,
                log.EntityId,
                log.OldValuesJson,
                log.NewValuesJson,
                log.CreatedAt,
                log.PreviousHash
            );

            if (log.CurrentHash != calculatedHash)
            {
                return false; // Tampered row content
            }

            expectedPrev = log.CurrentHash;
        }

        return true;
    }
}
