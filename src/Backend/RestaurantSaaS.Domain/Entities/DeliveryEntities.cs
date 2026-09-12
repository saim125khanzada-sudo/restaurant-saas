using System;
using RestaurantSaaS.Domain.Enums;
using RestaurantSaaS.SharedKernel.Common;
using RestaurantSaaS.SharedKernel.Interfaces;

namespace RestaurantSaaS.Domain.Entities;

public class DeliveryDispatch : BaseEntity, IMultiTenantEntity
{
    public Guid RestaurantId { get; set; }
    public Guid BranchId { get; set; }
    public Guid OrderId { get; set; }
    public Guid RiderId { get; set; }
    public DeliveryStatus Status { get; set; } = DeliveryStatus.Assigned;

    public string DeliveryAddress { get; set; } = string.Empty;
    public decimal? DestinationLatitude { get; set; }
    public decimal? DestinationLongitude { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerName { get; set; }

    public decimal CashToCollect { get; set; }
    public decimal CashCollected { get; set; }
    public bool IsCashCollected { get; set; }

    public DateTime? AssignedAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime? PickedUpAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string? FailureReason { get; set; }

    // Navigation
    public Order? Order { get; set; }
    public User? Rider { get; set; }
}

public class RiderLocationHistory : BaseEntity, IMultiTenantEntity
{
    public Guid RestaurantId { get; set; }
    public Guid BranchId { get; set; }
    public Guid RiderId { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public decimal? Heading { get; set; }
    public decimal? SpeedKmh { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
}

public class RiderCashReconciliation : BaseEntity, IMultiTenantEntity
{
    public Guid RestaurantId { get; set; }
    public Guid BranchId { get; set; }
    public Guid RiderId { get; set; }
    public DateTime ShiftDate { get; set; }

    public int TotalDeliveries { get; set; }
    public decimal TotalCashExpected { get; set; }
    public decimal TotalCashSubmitted { get; set; }
    public decimal DiscrepancyAmount { get; set; } // TotalCashSubmitted - TotalCashExpected

    public ReconciliationStatus Status { get; set; } = ReconciliationStatus.Pending;
    public Guid? ApprovedByUserId { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? Notes { get; set; }

    // Navigation
    public User? Rider { get; set; }
    public User? ApprovedByUser { get; set; }
}
