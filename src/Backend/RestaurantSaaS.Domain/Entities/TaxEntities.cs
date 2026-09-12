using System;
using RestaurantSaaS.Domain.Enums;
using RestaurantSaaS.SharedKernel.Common;
using RestaurantSaaS.SharedKernel.Interfaces;

namespace RestaurantSaaS.Domain.Entities;

public class TaxRule : BaseEntity, IMultiTenantEntity
{
    public Guid RestaurantId { get; set; }
    public Guid? BranchId { get; set; }
    public string Name { get; set; } = string.Empty; // e.g., "Sales Tax - Card (5%)", "Sales Tax - Cash (15%)"
    public decimal RatePercentage { get; set; }     // e.g., 5.00, 15.00, 16.00
    public OrderType? AppliesToOrderType { get; set; }
    public PaymentMethod? AppliesToPaymentMethod { get; set; } // Cash vs Card differential
    public bool IsActive { get; set; } = true;
    public string? Description { get; set; }
}

public class FiscalInvoiceRecord : BaseEntity, IMultiTenantEntity
{
    public Guid RestaurantId { get; set; }
    public Guid BranchId { get; set; }
    public Guid OrderId { get; set; }

    public string PosRegistrationNumber { get; set; } = string.Empty;
    public string FbrInvoiceNumber { get; set; } = string.Empty;
    public string QrCodeData { get; set; } = string.Empty;
    public FiscalSyncStatus Status { get; set; } = FiscalSyncStatus.Pending;

    public decimal TotalSalesValue { get; set; }
    public decimal TotalTaxCharged { get; set; }
    public string? ResponsePayload { get; set; }
    public DateTime? SyncedAt { get; set; }
    public int RetryCount { get; set; } = 0;

    // Navigation
    public Order? Order { get; set; }
}
