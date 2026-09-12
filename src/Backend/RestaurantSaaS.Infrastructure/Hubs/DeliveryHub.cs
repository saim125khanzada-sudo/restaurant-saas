using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using RestaurantSaaS.Application.Interfaces;
using RestaurantSaaS.Domain.Entities;

namespace RestaurantSaaS.Infrastructure.Hubs;

[Authorize]
public class DeliveryHub : Hub
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenantService _tenantService;

    public DeliveryHub(IApplicationDbContext context, ICurrentTenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    public async Task JoinBranchTracking(string branchId)
    {
        var groupName = $"branch_delivery_{branchId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    public async Task StreamRiderLocation(string branchId, decimal latitude, decimal longitude, decimal? heading, decimal? speedKmh)
    {
        var userId = _tenantService.UserId;
        if (userId == null) return;

        // Broadcast live coordinate to admin/manager tracking screens
        var groupName = $"branch_delivery_{branchId}";
        await Clients.Group(groupName).SendAsync("RiderLocationUpdated", new
        {
            riderId = userId.Value,
            branchId,
            latitude,
            longitude,
            heading,
            speedKmh,
            timestamp = DateTime.UtcNow
        });

        // Store location telemetry in PostgreSQL
        var history = new RiderLocationHistory
        {
            RestaurantId = _tenantService.RestaurantId ?? Guid.Empty,
            BranchId = Guid.TryParse(branchId, out var bGuid) ? bGuid : Guid.Empty,
            RiderId = userId.Value,
            Latitude = latitude,
            Longitude = longitude,
            Heading = heading,
            SpeedKmh = speedKmh,
            RecordedAt = DateTime.UtcNow
        };

        _context.RiderLocationHistories.Add(history);
        await _context.SaveChangesAsync();
    }
}
