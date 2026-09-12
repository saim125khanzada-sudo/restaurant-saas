using System;
using System.Collections.Generic;
using RestaurantSaaS.SharedKernel.Common;
using RestaurantSaaS.SharedKernel.Interfaces;

namespace RestaurantSaaS.Domain.Entities;

public enum SubscriptionStatus
{
    Active = 1,
    Trial = 2,
    PastDue = 3,
    Cancelled = 4
}

public enum InvoiceStatus
{
    Pending = 1,
    Paid = 2,
    Failed = 3
}

public class SubscriptionPlan : BaseEntity
{
    public string Name { get; set; } = string.Empty; // Basic, Pro, Enterprise
    public decimal MonthlyPrice { get; set; }
    public int MaxBranches { get; set; } = 1;
    public int MaxUsersPerBranch { get; set; } = 5;
    public bool HasAdvancedAnalytics { get; set; } = false;
    public bool HasFbrIntegration { get; set; } = false;
    public bool IsActive { get; set; } = true;

    // Navigation
    public virtual ICollection<TenantSubscription> Subscriptions { get; set; } = new List<TenantSubscription>();
}

public class TenantSubscription : BaseEntity, IMultiTenantEntity
{
    public Guid RestaurantId { get; set; }
    public Guid PlanId { get; set; }
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public bool AutoRenew { get; set; } = true;

    // Navigation
    public virtual SubscriptionPlan Plan { get; set; } = null!;
    public virtual Restaurant Restaurant { get; set; } = null!;
    public virtual ICollection<SubscriptionInvoice> Invoices { get; set; } = new List<SubscriptionInvoice>();
}

public class SubscriptionInvoice : BaseEntity, IMultiTenantEntity
{
    public Guid RestaurantId { get; set; }
    public Guid TenantSubscriptionId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? PaidAt { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Pending;
    public string? PaymentReference { get; set; }

    // Navigation
    public virtual TenantSubscription Subscription { get; set; } = null!;
}
