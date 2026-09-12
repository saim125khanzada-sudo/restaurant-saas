using MediatR;
using Microsoft.EntityFrameworkCore;
using RestaurantSaaS.Application.DTOs;
using RestaurantSaaS.Application.Interfaces;
using RestaurantSaaS.Domain.Entities;
using RestaurantSaaS.SharedKernel.Common;

namespace RestaurantSaaS.Application.Features.Catalog.Commands;

// 1. Categories
public record CreateCategoryCommand(CreateCategoryRequest Request) : IRequest<Result<CategoryDto>>;
public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Result<CategoryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenantService _tenantService;

    public CreateCategoryCommandHandler(IApplicationDbContext context, ICurrentTenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    public async Task<Result<CategoryDto>> Handle(CreateCategoryCommand command, CancellationToken cancellationToken)
    {
        if (!_tenantService.RestaurantId.HasValue)
            return Result<CategoryDto>.Failure("Tenant context is required.");

        var req = command.Request;
        var category = new Category
        {
            RestaurantId = _tenantService.RestaurantId.Value,
            ParentCategoryId = req.ParentCategoryId,
            Name = req.Name.Trim(),
            Description = req.Description?.Trim(),
            ImageUrl = req.ImageUrl,
            DisplayOrder = req.DisplayOrder,
            IsActive = true
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<CategoryDto>.Success(new CategoryDto(
            category.Id,
            category.Name,
            category.Description,
            category.ImageUrl,
            category.DisplayOrder,
            category.IsActive
        ));
    }
}

public record GetCategoriesQuery() : IRequest<Result<List<CategoryDto>>>;
public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, Result<List<CategoryDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetCategoriesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<CategoryDto>>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _context.Categories
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .Select(c => new CategoryDto(c.Id, c.Name, c.Description, c.ImageUrl, c.DisplayOrder, c.IsActive))
            .ToListAsync(cancellationToken);

        return Result<List<CategoryDto>>.Success(categories);
    }
}

// 2. Products
public record CreateProductCommand(CreateProductRequest Request) : IRequest<Result<ProductDto>>;
public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<ProductDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenantService _tenantService;

    public CreateProductCommandHandler(IApplicationDbContext context, ICurrentTenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    public async Task<Result<ProductDto>> Handle(CreateProductCommand command, CancellationToken cancellationToken)
    {
        if (!_tenantService.RestaurantId.HasValue)
            return Result<ProductDto>.Failure("Tenant context is required.");

        var req = command.Request;
        var category = await _context.Categories.FindAsync(new object[] { req.CategoryId }, cancellationToken);
        if (category == null)
            return Result<ProductDto>.Failure("Category does not exist.");

        var normalizedSku = req.SKU.Trim().ToUpperInvariant();
        if (await _context.Products.AnyAsync(p => p.SKU == normalizedSku, cancellationToken))
            return Result<ProductDto>.Failure($"Product with SKU '{req.SKU}' already exists.");

        var product = new Product
        {
            RestaurantId = _tenantService.RestaurantId.Value,
            CategoryId = req.CategoryId,
            Name = req.Name.Trim(),
            Description = req.Description?.Trim(),
            SKU = normalizedSku,
            BasePrice = req.BasePrice,
            CostPrice = req.CostPrice,
            ImageUrl = req.ImageUrl,
            IsAvailable = true,
            IsTaxExempt = req.IsTaxExempt,
            TaxRatePercentage = req.TaxRatePercentage
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<ProductDto>.Success(new ProductDto(
            product.Id,
            product.CategoryId,
            category.Name,
            product.Name,
            product.Description,
            product.SKU,
            product.BasePrice,
            product.CostPrice,
            product.ImageUrl,
            product.IsAvailable,
            product.IsTaxExempt,
            product.TaxRatePercentage,
            new List<ProductVariantDto>(),
            new List<ProductAddonDto>()
        ));
    }
}

public record GetProductsQuery(Guid? CategoryId) : IRequest<Result<List<ProductDto>>>;
public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, Result<List<ProductDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetProductsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<ProductDto>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Variants)
            .Include(p => p.ProductAddons)
                .ThenInclude(pa => pa.Addon)
            .AsQueryable();

        if (request.CategoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == request.CategoryId.Value);
        }

        var products = await query
            .OrderBy(p => p.Name)
            .Select(p => new ProductDto(
                p.Id,
                p.CategoryId,
                p.Category.Name,
                p.Name,
                p.Description,
                p.SKU,
                p.BasePrice,
                p.CostPrice,
                p.ImageUrl,
                p.IsAvailable,
                p.IsTaxExempt,
                p.TaxRatePercentage,
                p.Variants.Select(v => new ProductVariantDto(v.Id, v.Name, v.SKU, v.Price, v.CostPrice, v.IsAvailable)).ToList(),
                p.ProductAddons.Select(pa => new ProductAddonDto(pa.AddonId, pa.Addon.Name, pa.Addon.Price, pa.MinQuantity, pa.MaxQuantity, pa.IsRequired)).ToList()
            ))
            .ToListAsync(cancellationToken);

        return Result<List<ProductDto>>.Success(products);
    }
}
