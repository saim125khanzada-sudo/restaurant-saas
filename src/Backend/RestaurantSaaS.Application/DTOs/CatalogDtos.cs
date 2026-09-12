using RestaurantSaaS.Domain.Enums;

namespace RestaurantSaaS.Application.DTOs;

public record CategoryDto(Guid Id, string Name, string? Description, string? ImageUrl, int DisplayOrder, bool IsActive);
public record CreateCategoryRequest(string Name, string? Description, string? ImageUrl, int DisplayOrder, Guid? ParentCategoryId);

public record ProductVariantDto(Guid Id, string Name, string? SKU, decimal Price, decimal CostPrice, bool IsAvailable);
public record ProductAddonDto(Guid AddonId, string Name, decimal Price, int MinQuantity, int MaxQuantity, bool IsRequired);

public record ProductDto(
    Guid Id,
    Guid CategoryId,
    string CategoryName,
    string Name,
    string? Description,
    string SKU,
    decimal BasePrice,
    decimal CostPrice,
    string? ImageUrl,
    bool IsAvailable,
    bool IsTaxExempt,
    decimal TaxRatePercentage,
    List<ProductVariantDto> Variants,
    List<ProductAddonDto> Addons
);

public record CreateProductRequest(
    Guid CategoryId,
    string Name,
    string? Description,
    string SKU,
    decimal BasePrice,
    decimal CostPrice,
    string? ImageUrl,
    bool IsTaxExempt,
    decimal TaxRatePercentage
);

public record CreateProductVariantRequest(string Name, string? SKU, decimal Price, decimal CostPrice);
public record CreateAddonRequest(string Name, decimal Price, decimal CostPrice);

public record FloorSectionDto(Guid Id, Guid? BranchId, string Name, string? Description, int DisplayOrder);
public record CreateFloorSectionRequest(Guid? BranchId, string Name, string? Description, int DisplayOrder);

public record TableDto(Guid Id, Guid? BranchId, Guid? FloorSectionId, string TableNumber, int Capacity, TableStatus Status, Guid? CurrentOrderId);
public record CreateTableRequest(Guid? BranchId, Guid? FloorSectionId, string TableNumber, int Capacity);

public record BranchDto(Guid Id, Guid RestaurantId, string BranchName, string BranchCode, string Address, string Phone, decimal? Latitude, decimal? Longitude, bool IsActive);
public record CreateBranchRequest(string BranchName, string BranchCode, string Address, string Phone, decimal? Latitude, decimal? Longitude);
public record UpdateRestaurantProfileRequest(string Name, string? LogoUrl, string? Website, string ContactEmail, string ContactPhone);
