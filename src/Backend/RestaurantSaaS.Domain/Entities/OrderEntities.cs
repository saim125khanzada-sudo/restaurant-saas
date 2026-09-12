using RestaurantSaaS.SharedKernel.Common;
using RestaurantSaaS.SharedKernel.Interfaces;
using RestaurantSaaS.Domain.Enums;

namespace RestaurantSaaS.Domain.Entities;

public class Order : BaseEntity, IBranchScopedEntity
{
    public Guid RestaurantId { get; set; }
    public Guid? BranchId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public OrderType OrderType { get; set; } = OrderType.DineIn;
    public OrderStatus Status { get; set; } = OrderStatus.Created;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;

    // Associations
    public Guid? TableId { get; set; }
    public Guid? WaiterId { get; set; }
    public Guid? RiderId { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? DeliveryAddress { get; set; }

    // Financials
    public decimal Subtotal { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal DiscountTotal { get; set; }
    public decimal GrandTotal { get; set; }
    public string? DiscountReason { get; set; }

    // Idempotency
    public Guid IdempotencyKey { get; set; }

    // Navigation
    public virtual Branch? Branch { get; set; }
    public virtual RestaurantTable? Table { get; set; }
    public virtual User? Waiter { get; set; }
    public virtual User? Rider { get; set; }
    public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public virtual ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

public class OrderItem : BaseEntity, IMultiTenantEntity
{
    public Guid RestaurantId { get; set; }
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public Guid? ProductVariantId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public string? KitchenNotes { get; set; }

    public virtual Order Order { get; set; } = null!;
    public virtual ICollection<OrderItemAddon> Addons { get; set; } = new List<OrderItemAddon>();
}

public class OrderItemAddon : BaseEntity, IMultiTenantEntity
{
    public Guid RestaurantId { get; set; }
    public Guid OrderItemId { get; set; }
    public Guid AddonId { get; set; }
    public string AddonName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal TotalPrice { get; set; }

    public virtual OrderItem OrderItem { get; set; } = null!;
}

public class OrderStatusHistory : BaseEntity, IBranchScopedEntity
{
    public Guid RestaurantId { get; set; }
    public Guid? BranchId { get; set; }
    public Guid OrderId { get; set; }
    public OrderStatus PreviousStatus { get; set; }
    public OrderStatus NewStatus { get; set; }
    public Guid? ChangedByUserId { get; set; }
    public string? Reason { get; set; }

    public virtual Order Order { get; set; } = null!;
}

public class Payment : BaseEntity, IBranchScopedEntity
{
    public Guid RestaurantId { get; set; }
    public Guid? BranchId { get; set; }
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; } = PaymentMethod.Cash;
    public string? TransactionReference { get; set; }
    public DateTimeOffset ProcessedAt { get; set; } = DateTimeOffset.UtcNow;

    public virtual Order Order { get; set; } = null!;
}
