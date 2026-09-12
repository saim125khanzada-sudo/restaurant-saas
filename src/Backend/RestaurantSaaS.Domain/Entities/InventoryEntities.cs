using System;
using System.Collections.Generic;
using RestaurantSaaS.Domain.Enums;
using RestaurantSaaS.SharedKernel.Common;
using RestaurantSaaS.SharedKernel.Interfaces;

namespace RestaurantSaaS.Domain.Entities;

public class Vendor : BaseEntity, IMultiTenantEntity
{
    public Guid RestaurantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? TaxNumber { get; set; }
    public bool IsActive { get; set; } = true;
}

public class Ingredient : BaseEntity, IMultiTenantEntity
{
    public Guid RestaurantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string UnitOfMeasure { get; set; } = "kg"; // kg, liter, piece, etc.
    public decimal ReorderThreshold { get; set; }
    public decimal CostPerUnit { get; set; }
    public bool IsActive { get; set; } = true;
}

public class RecipeItem : BaseEntity, IMultiTenantEntity
{
    public Guid RestaurantId { get; set; }
    public Guid ProductId { get; set; }
    public Guid? ProductVariantId { get; set; }
    public Guid IngredientId { get; set; }
    public decimal QuantityRequired { get; set; }

    // Navigation
    public Product? Product { get; set; }
    public ProductVariant? ProductVariant { get; set; }
    public Ingredient? Ingredient { get; set; }
}

public class StockLevel : BaseEntity, IMultiTenantEntity
{
    public Guid RestaurantId { get; set; }
    public Guid BranchId { get; set; }
    public Guid IngredientId { get; set; }
    public decimal QuantityOnHand { get; set; }

    // Navigation
    public Branch? Branch { get; set; }
    public Ingredient? Ingredient { get; set; }
}

public class StockMovement : BaseEntity, IMultiTenantEntity
{
    public Guid RestaurantId { get; set; }
    public Guid BranchId { get; set; }
    public Guid IngredientId { get; set; }
    public StockMovementType MovementType { get; set; }
    public decimal Quantity { get; set; } // positive for addition, negative for deduction
    public decimal UnitCost { get; set; }
    public Guid? ReferenceOrderId { get; set; }
    public Guid? ReferencePurchaseOrderId { get; set; }
    public string? Reason { get; set; }

    // Navigation
    public Ingredient? Ingredient { get; set; }
}

public class PurchaseOrder : BaseEntity, IMultiTenantEntity
{
    public Guid RestaurantId { get; set; }
    public Guid BranchId { get; set; }
    public Guid VendorId { get; set; }
    public string PoNumber { get; set; } = string.Empty;
    public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Draft;
    public decimal TotalAmount { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public DateTime? ReceivedAt { get; set; }
    public string? Notes { get; set; }

    // Navigation
    public Vendor? Vendor { get; set; }
    public List<PurchaseOrderItem> Items { get; set; } = new();
}

public class PurchaseOrderItem : BaseEntity, IMultiTenantEntity
{
    public Guid RestaurantId { get; set; }
    public Guid PurchaseOrderId { get; set; }
    public Guid IngredientId { get; set; }
    public decimal QuantityOrdered { get; set; }
    public decimal QuantityReceived { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }

    // Navigation
    public Ingredient? Ingredient { get; set; }
}
