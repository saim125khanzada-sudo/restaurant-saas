using RestaurantSaaS.SharedKernel.Common;
using RestaurantSaaS.SharedKernel.Interfaces;

namespace RestaurantSaaS.Domain.Entities;

public class Category : BaseEntity, IMultiTenantEntity
{
    public Guid RestaurantId { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;

    // Navigation
    public virtual Category? ParentCategory { get; set; }
    public virtual ICollection<Category> SubCategories { get; set; } = new List<Category>();
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}

public class Product : BaseEntity, IMultiTenantEntity
{
    public Guid RestaurantId { get; set; }
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string SKU { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public decimal CostPrice { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsAvailable { get; set; } = true;
    public bool IsTaxExempt { get; set; } = false;
    public decimal TaxRatePercentage { get; set; } = 0;

    // Navigation
    public virtual Category Category { get; set; } = null!;
    public virtual ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    public virtual ICollection<ProductAddon> ProductAddons { get; set; } = new List<ProductAddon>();
}

public class ProductVariant : BaseEntity, IMultiTenantEntity
{
    public Guid RestaurantId { get; set; }
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? SKU { get; set; }
    public decimal Price { get; set; }
    public decimal CostPrice { get; set; }
    public bool IsAvailable { get; set; } = true;

    // Navigation
    public virtual Product Product { get; set; } = null!;
}

public class Addon : BaseEntity, IMultiTenantEntity
{
    public Guid RestaurantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal CostPrice { get; set; }
    public bool IsAvailable { get; set; } = true;

    // Navigation
    public virtual ICollection<ProductAddon> ProductAddons { get; set; } = new List<ProductAddon>();
}

public class ProductAddon : BaseEntity, IMultiTenantEntity
{
    public Guid RestaurantId { get; set; }
    public Guid ProductId { get; set; }
    public Guid AddonId { get; set; }
    public int MinQuantity { get; set; } = 0;
    public int MaxQuantity { get; set; } = 1;
    public bool IsRequired { get; set; } = false;

    // Navigation
    public virtual Product Product { get; set; } = null!;
    public virtual Addon Addon { get; set; } = null!;
}
